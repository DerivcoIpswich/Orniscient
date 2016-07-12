using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Extensions;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Derivco.Orniscient.Viewer.Hubs;
using Microsoft.AspNet.SignalR;
using Orleans;
using Orleans.Streams;

namespace Derivco.Orniscient.Viewer.Observers
{
    public class OrniscientObserver : IAsyncObserver<DiffModel>
    {
        private static readonly Lazy<OrniscientObserver> _instance = new Lazy<OrniscientObserver>(() => new OrniscientObserver());
        public static OrniscientObserver Instance => _instance.Value;
        private IAsyncStream<DiffModel> _stream;

        public Guid StreamId => _stream.Guid;

        public OrniscientObserver()
        {
            var streamprovider = GrainClient.GetStreamProvider("SMSProvider");
            _stream = streamprovider.GetStream<DiffModel>(Guid.Empty, "OrniscientClient");
            _stream.SubscribeAsync(this);
        }

        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null,int sessionId=0)
        {
            var _dashboardInstanceGrain = GrainClient.GrainFactory.GetGrain<IDashboardInstanceGrain>(sessionId);
            var diffmodel = await _dashboardInstanceGrain.GetAll(filter);
            return diffmodel;
        }

        public Task OnNextAsync(DiffModel item, StreamSequenceToken token = null)
        {
            if (item != null)
            {
                //Debug.WriteLine($"OrniscientObserver (GrainsUpdated) -  DiffModel Session ID: {item.SessionId} ,Stream Id :{StreamId},DiffModelSent on Stream : {item.StreamIdIWasSentOn}, Types : {string.Join("|", item.NewGrains.Select(p => p.Type))}");
                GlobalHost.ConnectionManager.GetHubContext<OrniscientHub>().Clients.Group(item.SessionId.ToString()).grainActivationChanged(item);
            }
            return TaskDone.Done;
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }
    }
}

