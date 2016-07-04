using System;
using System.Collections.Generic;

namespace Derivco.Orniscient.Viewer.Observers
{
    public class OrniscientObserverContainer
    {
        private static readonly Lazy<OrniscientObserverContainer> _instance = new Lazy<OrniscientObserverContainer>(()=>new OrniscientObserverContainer());
        public static OrniscientObserverContainer Instance => _instance.Value;

        private readonly Dictionary<int,OrniscientObserver> _observers = new Dictionary<int, OrniscientObserver>();

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