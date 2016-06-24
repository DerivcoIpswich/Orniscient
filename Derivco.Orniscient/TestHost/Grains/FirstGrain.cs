using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.Streams;

namespace TestHost.Grains
{
    [OrniscientGrain]
    public class FirstGrain : Grain, IFirstGrain
    {
        private IStreamProvider _streamProvider;
        public override Task OnActivateAsync()
        {
            _streamProvider = GetStreamProvider("SMSProvider");
            RegisterTimer(p => AddGrains(10) ,null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15));
            return base.OnActivateAsync();
        }

        public async Task KeepAlive()
        {
            await AddGrains(100);
        }

        private async Task AddGrains(int grainCountToAdd = 10)
        {
            for (var i = 0; i < grainCountToAdd; i++)
            {
                var grainId = Guid.NewGuid();
                await _streamProvider.GetStream<Guid>(grainId, "TestStream").OnNextAsync(grainId);
            }
        }
    }
}