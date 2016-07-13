using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;

namespace TestGrains.Grains
{
    [OrniscientGrain(typeof(SubGrain), LinkType.SameId, "lightblue")]
    public class FooGrain : Grain, IFooGrain, IFilterableGrain
    {
        private FilterRow[] _filters;

        public override async Task OnActivateAsync()
        {
            //creating dummy filters for testing
            string[] _sports = new[] {"Rugby", "Soccer", "Pool" };//, "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            this._filters = new[]
            {
                new FilterRow() {FilterName = "sport", Value =_sports[rand.Next(0,2)]},
                new FilterRow() {FilterName = "league", Value = $"some league name"} //include the id here, just to see the difference
            };

            await base.OnActivateAsync();
        }

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(_filters);
        }
    }
}