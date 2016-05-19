using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy;
using Derivco.Orniscient.Proxy.Observers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Orleans;

namespace Derivco.Orniscient.Viewer.Hubs
{
    [HubName("orniscientHub")]
    public class OrniscientHub : Hub
    {
        private readonly OrniscientObserver _orniscientObserver;

        public OrniscientHub(OrniscientObserver orniscientObserver)
        {
            _orniscientObserver = orniscientObserver;
        }

        public OrniscientHub()
            :this(OrniscientObserver.Instance)
        {}

        public async Task<List<UpdateModel>> GetCurrentSnapshot()
        {
            return await _orniscientObserver.GetCurrentSnapshot();
        }
    }

    public class OrniscientObserver : IOrniscientObserver
    {
        private static readonly Lazy<OrniscientObserver> _instance = new Lazy<OrniscientObserver>(() => new OrniscientObserver());
        private IOrniscientObserver observer;

        private OrniscientObserver()
        {
            var _reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientGrain>(Guid.Empty);
            observer = GrainClient.GrainFactory.CreateObjectReference<IOrniscientObserver>(this).Result;
            _reportingGrain.Subscribe(observer);
        }

        public static OrniscientObserver Instance => _instance.Value;

        public async Task<List<UpdateModel>> GetCurrentSnapshot()
        {
            var orniscientGrain = GrainClient.GrainFactory.GetGrain<IOrniscientGrain>(Guid.Empty);
            var rr = (await orniscientGrain.GetAll());
            return rr;
        }

        public void GrainsUpdated(DiffModel model)
        {
            Debug.WriteLine($"Pushing down {model.NewGrains.Count} new grains and removing {model.RemovedGrains.Count}");
            GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.All.grainActivationChanged(model);
        }
    }
}