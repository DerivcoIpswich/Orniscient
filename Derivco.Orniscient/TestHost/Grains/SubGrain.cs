using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;
using Orleans.Streams;

namespace TestHost.Grains
{
    [ImplicitStreamSubscription("TestStream")]
    [Derivco.Orniscient.Proxy.Attributes.OrniscientGrain(linkFromType: typeof(TestHost.Grains.FirstGrain),linkType:LinkType.SingleInstance,colour:"yellow")]
    public class SubGrain : Grain, TestHost.Grains.ISubGrain, IAsyncObserver<Guid> 
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

            var t = GrainFactory.GetGrain<TestHost.Grains.IFooGrain>(item);
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

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(new FilterRow[] {});
        }
    }
}