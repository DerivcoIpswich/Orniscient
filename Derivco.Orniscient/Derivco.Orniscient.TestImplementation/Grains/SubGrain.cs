using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.Streams;

namespace Derivco.Orniscient.TestImplementation.Grains
{
    [ImplicitStreamSubscription("TestStream")]
    [Proxy.Attributes.OrniscientGrain(linkFromType: typeof(FirstGrain),linkType:LinkType.SingleInstance,colour:"yellow")]
    public class SubGrain : Grain, ISubGrain, IAsyncObserver<Guid> 
    {
        private StreamSubscriptionHandle<Guid> _subscriptionHandle;

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider("SMSProvider");
            var incomingStream = streamProvider.GetStream<Guid>(this.GetPrimaryKey(), "TestStream");
            _subscriptionHandle = await incomingStream.SubscribeAsync((IAsyncObserver<Guid>)this);
            await base.OnActivateAsync();
        }


        public Task SayHallo()
        {
            return TaskDone.Done;
        }

        public async Task OnNextAsync(Guid item, StreamSequenceToken token = null)
        {
            var msg = $"Hallo from Grain : {this.GetPrimaryKey()}";
            Console.WriteLine(msg);

            var t = GrainFactory.GetGrain<IFooGrain>(item);
            await t.KeepAlive();

            await Task.FromResult(msg);
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }
    }
}