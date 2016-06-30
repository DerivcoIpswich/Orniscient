using System;
using System.Diagnostics;
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
        private FilterRow[] filters;

        public override async Task OnActivateAsync()
        {
            //creating dummy filters for testing
            string[] _sports = new[] {"Rugby", "Soccer", "Pool" };//, "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            this.filters = new[]
            {
                new FilterRow() {FilterName = "sport", Value =_sports[rand.Next(0,2)]},
                new FilterRow() {FilterName = "league", Value = $"some league name"} //include the id here, just to see the difference
            };

            //temp
            //var filterGrain = GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
            //await filterGrain.RegisterFilter(this.GetType().FullName, this.GetPrimaryKey().ToString(), filters);

            //RegisterTimer(SetFilters, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));

            await base.OnActivateAsync();
        }


        //private async Task SetFilters(object o)
        //{
        //    var filterGrain = GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
        //    await filterGrain.RegisterFilter(this.GetType().FullName, this.GetPrimaryKey().ToString(), filters);
        //}



        public Task KeepAlive()
        {
            Debug.WriteLine("Keep alive called on foograin");
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(filters);
        }
    }
}