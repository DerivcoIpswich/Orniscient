using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Viewer.Observers
{
    public class OrniscientObserverContainer
    {
        private static readonly Lazy<OrniscientObserverContainer> _instance = new Lazy<OrniscientObserverContainer>(()=>new OrniscientObserverContainer());
        public static OrniscientObserverContainer Instance => _instance.Value;

        private readonly Dictionary<int,OrniscientObserver> _observers = new Dictionary<int, OrniscientObserver>();

        public async Task SetTypeFilter(Func<GrainType, bool> filter)
        {
            if (filter != null)
            {
                var dashboardCollecterGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
                var availableTypes = await dashboardCollecterGrain.GetGrainTypes();
                await dashboardCollecterGrain.SetTypeFilter(availableTypes.Where(filter).ToArray());
            }
        }


        public OrniscientObserver Get(int sessionId)
        {
            if (!_observers.ContainsKey(sessionId))
            {
                _observers.Add(sessionId,new OrniscientObserver(sessionId));
            }
            return _observers[sessionId];
        }
    }
}