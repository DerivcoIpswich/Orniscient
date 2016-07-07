using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
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
        public int SessionId { get; set; }
        private readonly IOrniscientObserver _observer;
        private readonly IDashboardInstanceGrain _dashboardInstanceGrain;

        public OrniscientObserver(int sessionId)
        {
            SessionId = sessionId;
            _dashboardInstanceGrain = GrainClient.GrainFactory.GetGrain<IDashboardInstanceGrain>(sessionId);
            _observer = GrainClient.GrainFactory.CreateObjectReference<IOrniscientObserver>(this).Result;
            _dashboardInstanceGrain.Subscribe(_observer);

        }

        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null)
        {
            var diffmodel = await _dashboardInstanceGrain.GetAll(filter);
            return diffmodel;
        }

        public void GrainsUpdated(DiffModel model)
        {
            //TODO : when we are in summray mode return all from the observer....
            if (model != null)
            {
                Debug.WriteLine(
                    $"Pushing down {model.NewGrains.Count} new grains and removing {model.RemovedGrains.Count}");


                model.SentDate = DateTime.Now;


                //TODO : Only push down to the asking user.
                //GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.User(SessionId.ToString()).grainActivationChanged(model);
                GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.All.grainActivationChanged(model);
            }
        }
    }
}

