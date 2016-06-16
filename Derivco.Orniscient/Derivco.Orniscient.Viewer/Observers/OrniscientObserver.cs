using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Derivco.Orniscient.Viewer.Hubs;
using Microsoft.AspNet.SignalR;
using Orleans;

namespace Derivco.Orniscient.Viewer.Observers
{
    public class OrniscientObserver : IOrniscientObserver
    {
        private static readonly Lazy<OrniscientObserver> _instance = new Lazy<OrniscientObserver>(() => new OrniscientObserver());
        private IOrniscientObserver observer;

        private OrniscientObserver()
        {
            //Proper async code needed ?????????
            var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            observer = GrainClient.GrainFactory.CreateObjectReference<IOrniscientObserver>(this).Result;
            orniscientGrain.Subscribe(observer);
        }

        public async Task SetTypeFilter(Func<string, bool> filter)
        {
            if (filter != null)
            {
                var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
                var availableTypes = await orniscientGrain.GetGrainTypes();
                await orniscientGrain.SetTypeFilter(availableTypes.Where(filter).ToArray());
            }
        }

        public static OrniscientObserver Instance => _instance.Value;

        public async Task<List<UpdateModel>> GetCurrentSnapshot(AppliedFilter filter = null)
        {
            var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var grains = await orniscientGrain.GetAll();
            if (filter == null)
                return grains;

            //order of filtering applies here.
            //1. Grain Id
            if (!string.IsNullOrEmpty(filter.GrainId))
            {
                return grains.Where(p => p.Guid.ToString().Contains(filter.GrainId)).ToList();
            }

            //2. Silo
            var grainQuery = grains.Where(p => filter.SelectedSilos == null || filter.SelectedSilos.Length==0 || filter.SelectedSilos.Contains(p.Silo));

            //3. Apply Type Filters

            if (filter.TypeFilters != null && filter.TypeFilters.Any())
            {
                //we need to dynamically build up the expression tree here
                


                grainQuery = grainQuery.Where(p => filter.TypeFilters.Any(t => t.TypeName == p.Type));

                var grainIdsToFilter = new List<string>();
                foreach (var appliedTypeFilter in filter.TypeFilters)
                {
                    if (appliedTypeFilter.SelectedValues != null)
                    {
                        var typeFilterGrain = GrainClient.GrainFactory.GetGrain<ITypeFilterGrain>(appliedTypeFilter.TypeName);
                        grainIdsToFilter.AddRange(await typeFilterGrain.GetGrainIdsForFilter(appliedTypeFilter));
                    }
                }
                if (grainIdsToFilter.Any())
                {
                    grainQuery = grainQuery.Where(p => grainIdsToFilter.Contains(p.Guid.ToString()));
                }
            }
            return grainQuery.ToList();
        }

        public void GrainsUpdated(DiffModel model)
        {
            Debug.WriteLine($"Pushing down {model.NewGrains.Count} new grains and removing {model.RemovedGrains.Count}");
            GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.All.grainActivationChanged(model);
        }
    }
}