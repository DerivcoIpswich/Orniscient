using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;
using Orleans.Streams;

namespace TestHost.Grains
{
    [OrniscientGrain]
    public class FirstGrain : Grain, IFirstGrain, IFilterableGrain
    {
        private IStreamProvider _streamProvider;
        public override Task OnActivateAsync()
        {
            _streamProvider = GetStreamProvider("SMSProvider");
            //RegisterTimer(p => AddGrains(10) ,null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15));
            return base.OnActivateAsync();
        }

        public async Task KeepAlive()
        {
            Console.WriteLine("Hi, I am your first Grain");
            await AddGrains(20);
            //return TaskDone.Done;
        }

        private async Task AddGrains(int grainCountToAdd=10)
        {
            for (var i = 0; i < grainCountToAdd; i++)
            {
                var temp = Guid.NewGuid();
                await _streamProvider.GetStream<Guid>(temp, "TestStream").OnNextAsync(temp);
            }
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(new FilterRow[]
            {
                new FilterRow() {Name = "test", Value = "test"}
            });

        }
    }
}