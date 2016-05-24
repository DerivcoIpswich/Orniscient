using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

namespace TestHost.Grains
{
    [Derivco.Orniscient.Proxy.Attributes.OrniscientGrain]
    public class FirstGrain : Grain, IFirstGrain
    {
        private IStreamProvider _streamProvider;
        public override Task OnActivateAsync()
        {
            _streamProvider = GetStreamProvider("SMSProvider");
           // RegisterTimer(p => AddGrains() ,null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15));
            return base.OnActivateAsync();
        }

        public async Task KeepAlive()
        {
            Console.WriteLine("Hi, I am your first Grain");
            await AddGrains();
            //return TaskDone.Done;
        }

        private async Task AddGrains()
        {
            for (var i = 0; i < 50; i++)
            {
                var temp = Guid.NewGuid();
                await _streamProvider.GetStream<Guid>(temp, "TestStream").OnNextAsync(temp);
            }
        }
    }
}