using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private AppliedFilter _currentFilter ;

        private OrniscientObserver()
        {
            
            //Proper async code needed ?????????
            var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            observer = GrainClient.GrainFactory.CreateObjectReference<IOrniscientObserver>(this).Result;
            orniscientGrain.Subscribe(observer);
        }

        public async Task SetTypeFilter(Func<GrainType, bool> filter)
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
            _currentFilter = filter;
            return await ApplyFilter(grains);
        }

        public void GrainsUpdated(DiffModel model)
        {
            if (model != null)
            {
                //////This is will be blocking the thread until execution is finished, but that is ok for now, we do not expect allot of users on the dashboard :-)
                //if (model.NewGrains.Any())
                //{
                //    model.NewGrains = ApplyFilter(model.NewGrains).Result;
                //}
                Debug.WriteLine($"Pushing down {model.NewGrains.Count} new grains and removing {model.RemovedGrains.Count}");
                GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.All.grainActivationChanged(model);
            }
        }

        private async Task<List<UpdateModel>> ApplyFilter(List<UpdateModel> grains)
        {
            if (_currentFilter== null)
                return grains;

            //order of filtering applies here.
            //1. Grain Id
            if (!string.IsNullOrEmpty(_currentFilter.GrainId))
            {
                return grains.Where(p => p.Guid.ToString().Contains(_currentFilter.GrainId)).ToList();
            }

            //2. Silo
            var grainQuery = grains.Where(p => _currentFilter.SelectedSilos == null || _currentFilter.SelectedSilos.Length == 0 || _currentFilter.SelectedSilos.Contains(p.Silo));

            //3. Apply Type Filters

            if (_currentFilter.TypeFilters != null && _currentFilter.TypeFilters.Any())
            {
                //TODO : we need to dynamically build up the expression tree here

                grainQuery = grainQuery.Where(p => _currentFilter.TypeFilters.Any(t => t.TypeName == p.Type));

                var grainIdsToFilter = new List<string>();
                foreach (var appliedTypeFilter in _currentFilter.TypeFilters)
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

    }
}