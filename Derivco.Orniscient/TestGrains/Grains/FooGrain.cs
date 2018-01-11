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
    public class FooGrain : Grain, IFooGrain , IFilterable 
    {
        private FilterRow[] _filters;

        public override async Task OnActivateAsync()
        {
            //creating dummy filters for testing
            var sports = new[] {"Rugby", "Soccer",  "Pool", "Darts", "Formula 1", "Horse Racing" };
            var rand = new Random();
            _filters = new[]
            {
                new FilterRow {FilterName = "Sport", Value =sports[rand.Next(0,5)]},
                new FilterRow {FilterName = "League", Value = "Some League Name"} //include the id here, just to see the difference
            };
            Debug.WriteLine($"FooGrain started : {this.GetPrimaryKey()}");
            await base.OnActivateAsync();
        }

        [OrniscientMethod]
        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        [OrniscientMethod]
        public Task<Dictionary<string, string>> KeepAliveOne(int? intOne, string stringOne)
        {
            Debug.WriteLine($"KeepAliveOne called on {this.GetPrimaryKeyString()}. intOne: {intOne} stringOne: {stringOne}");
            return Task.FromResult(new Dictionary<string, string> {{"keyone", this.GetPrimaryKey().ToString() }, {"keytwo", stringOne}});
        }

        [OrniscientMethod]
        public Task KeepAliveOne(int intOne, int stringOne)
        {
            Debug.WriteLine("KeepAliveOne called.");
            return TaskDone.Done;
        }

        [OrniscientMethod]
        public Task KeepAliveTwo(ExternalParameterType externalParameterTwo)
        {
            Debug.WriteLine("KeepAliveTwo called.");
            return TaskDone.Done;
        }

        [OrniscientMethod]
        public Task KeepAliveThree(Dictionary<string, int> dictionaryStringIntThree )
        {
            Debug.WriteLine("KeepAliveThree called.");
            return TaskDone.Done;
        }

        [OrniscientMethod]
        public Task KeepAliveThree(Dictionary<int, int> dictionaryStringIntThree)
        {
            Debug.WriteLine("KeepAliveThree called.");
            return TaskDone.Done;
        }

        [OrniscientMethod]
        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(_filters);
        }

        public override Task OnDeactivateAsync()
        {
            Debug.WriteLine("FooGrain Deactivated.");
            return Task.CompletedTask;
        }
    }
}