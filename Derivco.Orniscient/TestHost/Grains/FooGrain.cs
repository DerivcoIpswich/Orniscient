using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;

namespace TestHost.Grains
{
    [OrniscientGrain(typeof(SubGrain), LinkType.SameId, "lightblue")]
    public class FooGrain : Grain, IFooGrain, IFilterableGrain
    {
        


        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            string[] _sports = new[] { "Rugby", "Soccer", "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            return Task.FromResult(new[]
            {
                new FilterRow() {Name = "sport", Value =_sports[rand.Next(0,5)]},
                new FilterRow() {Name = "league", Value = $"some league name"} //include the id here, just to see the difference
            });
        }
    }
}