using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
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

        public async Task SetTypeFilter(Func<string,bool> filter)
        {
            if (filter != null)
            {
                var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
                var availableTypes = await orniscientGrain.GetGrainTypes();
                await orniscientGrain.SetTypeFilter(availableTypes.Where(filter).ToArray());
            }
        }



        public static OrniscientObserver Instance => _instance.Value;

        public async Task<List<UpdateModel>> GetCurrentSnapshot()
        {
            var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var temp = await orniscientGrain.GetAll();
            return temp;
        }

        public void GrainsUpdated(DiffModel model)
        {
            Debug.WriteLine($"Pushing down {model.NewGrains.Count} new grains and removing {model.RemovedGrains.Count}");
            GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.All.grainActivationChanged(model);
        }
    }
}