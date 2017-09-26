using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
	public interface ITestGrain : IGrainWithStringKey
	{
		Task<int> TestOrniscientMethodOne(int one);
		Task<string> TestOrniscientMethodTwo();
		Task TestNonOrniscientMethod(int paramBra);
		Task KeepAlive();
	}

	public class TestGrain : Grain, ITestGrain
	{
		[OrniscientMethod]
		public Task<int> TestOrniscientMethodOne(int one)
		{
			return Task.FromResult(one * one);
		}

		[OrniscientMethod]
		public Task<string> TestOrniscientMethodTwo()
		{
			return Task.FromResult("Great Success");
		}

		public Task TestNonOrniscientMethod(int paramBra)
		{
			return TaskDone.Done;
		}

		public Task KeepAlive()
		{
			return TaskDone.Done;
		}
	}
}