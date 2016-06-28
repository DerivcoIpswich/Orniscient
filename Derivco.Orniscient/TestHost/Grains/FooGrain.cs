using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Microsoft.CodeAnalysis.CSharp;
using Orleans;

namespace TestHost.Grains
{
    [OrniscientGrain(typeof(SubGrain), LinkType.SameId, "lightblue")]
    public class FooGrain : Grain, IFooGrain, IFilterableGrain
    {
        private FilterRow[] filters;

        public override async Task OnActivateAsync()
        {
            //creating dummy filters for testing
            string[] _sports = new[] {"Rugby", "Soccer", "Pool" };//, "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            this.filters = new[]
            {
                new FilterRow() {Name = "sport", Value =_sports[rand.Next(0,2)]},
                new FilterRow() {Name = "league", Value = $"some league name"} //include the id here, just to see the difference
            };


            //
            //var typeFilterGrain = GrainFactory.GetGrain<ITypeFilterGrain>(this.GetType().FullName);
            //await typeFilterGrain.RegisterFilter(await GetFilters(), this.GetPrimaryKey());


            await base.OnActivateAsync();
        }

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(filters);
        }
    }
}