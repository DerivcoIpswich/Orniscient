using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.Streams;

namespace TestGrains.Grains
{
    [OrniscientGrain]
    public class FirstGrain : Grain, IFirstGrain
    {
        private IStreamProvider _streamProvider;
        public override async Task OnActivateAsync()
        {
            _streamProvider = GetStreamProvider("SMSProvider");
            //RegisterTimer(p => AddGrains(10) ,null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));
            await base.OnActivateAsync();
        }

        public async Task KeepAlive()
        {
            await AddGrains(5);
        }

        private async Task AddGrains(int grainCountToAdd = 10)
        {
            //create bargrain 

            var random = new Random(5);
            await GrainFactory.GetGrain<IBarGrain>(8).KeepAlive();
            await GrainFactory.GetGrain<IBarGrain>(87).KeepAlive();
            await GrainFactory.GetGrain<IBarGrain>(20).KeepAlive();

            for (var i = 0; i < grainCountToAdd; i++)
            {
             
                var grainId = Guid.NewGuid();
                await _streamProvider.GetStream<Guid>(grainId, "TestStream").OnNextAsync(grainId);
            }
        }
    }
}