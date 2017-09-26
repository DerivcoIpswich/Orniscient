using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;
using Orleans.Streams;

namespace TestGrains.Grains
{
	[ImplicitStreamSubscription("TestStream")]
	[OrniscientGrain(linkFromType: typeof(FirstGrain), linkType: LinkType.SingleInstance, colour: "yellow", defaultLinkFromTypeId: "Hallo")]
	public class SubGrain : Grain, ISubGrain, IAsyncObserver<Guid>
	{
		private StreamSubscriptionHandle<Guid> _subscriptionHandle;

		public override async Task OnActivateAsync()
		{
			var streamProvider = GetStreamProvider("SMSProvider");
			var incomingStream = streamProvider.GetStream<Guid>(this.GetPrimaryKey(), "TestStream");
			_subscriptionHandle = await incomingStream.SubscribeAsync(this);
			await base.OnActivateAsync();
		}

		public Task SayHallo()
		{
			return TaskDone.Done;
		}

		public async Task OnNextAsync(Guid item, StreamSequenceToken token = null)
		{
			Console.WriteLine($"Grain started : {this.GetPrimaryKey()}");
			var t = GrainFactory.GetGrain<IFooGrain>(item);
			var s = GrainFactory.GetGrain<IAnotherFooGrain>(item);
			await s.KeepAliveOne(1, "Hello");
			await t.KeepAlive();
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
			return Task.FromResult(new[] { new FilterRow { FilterName = "Sub Filter", Value = "Test" } });
		}

		public Task KeepAlive()
		{
			return TaskDone.Done;
		}
	}
}