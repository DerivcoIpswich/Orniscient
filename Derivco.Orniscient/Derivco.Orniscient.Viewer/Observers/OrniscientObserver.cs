using System;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Models;
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
            var streamprovider = GrainClient.GetStreamProvider(StreamKeys.StreamProvider);
            _stream = streamprovider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientClient);
            _stream.SubscribeAsync(this);
        }

        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null,int sessionId=0)
        {
            var dashboardInstanceGrain = GrainClient.GrainFactory.GetGrain<IDashboardInstanceGrain>(sessionId);
            var diffmodel = await dashboardInstanceGrain.GetAll(filter);
            return diffmodel;
        }

        

        public Task OnNextAsync(DiffModel item, StreamSequenceToken token = null)
        {
            if (item != null)
            {
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

        public async Task SetTypeFilter(Func<GrainType, bool> filter)
        {
            if (filter != null)
            {
                var dashboardCollecterGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
                var availableTypes = await dashboardCollecterGrain.GetGrainTypes();
                await dashboardCollecterGrain.SetTypeFilter(availableTypes.Where(filter).ToArray());
            }
        }

    }
}

