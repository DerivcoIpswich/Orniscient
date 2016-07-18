using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var sports = new[] {"Rugby", "Soccer",  "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            this._filters = new[]
            {
                new FilterRow() {FilterName = "sport", Value =sports[rand.Next(0,5)]},
                new FilterRow() {FilterName = "league", Value = $"some league name"} //include the id here, just to see the difference
            };

            await base.OnActivateAsync();
        }

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task KeepAliveOne(int intOne, string stringOne)
        {
            Debug.WriteLine("one");
            return TaskDone.Done;
        }

        public Task KeepAliveTwo(bool boolTwo, List<string> listStringTwo, FilterRow filterRowTwo)
        {
            Debug.WriteLine("two");
            return TaskDone.Done;
        }

        public Task KeepAliveThree(Dictionary<string, int> dictionaryStringIntThree, Random randomThree, Guid guidThree )
        {
            Debug.WriteLine("three");
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(_filters);
        }
    }
}