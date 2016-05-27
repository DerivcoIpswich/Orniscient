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
        private string _sport;
        private string _league;

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(new[]
            {
                new FilterRow() {Name = "sport", Value = "rugby"},
                new FilterRow() {Name = "league", Value = $"rugby world cup"} //include the id here, just to see the difference
            });
        }
    }
}