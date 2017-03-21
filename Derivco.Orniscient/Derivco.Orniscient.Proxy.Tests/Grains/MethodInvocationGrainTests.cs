using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures;
using NSubstitute;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
	public class TypeMethodGrainTests : IClassFixture<TypeMethodGrainTestFixture>
	{
		private readonly MethodInvocationGrain _grain;
		private readonly IGrainFactory _grainFactory;
		public TypeMethodGrainTests()
		{
			var orleansIdentity = Substitute.For<IGrainIdentity>();
			var orleansRuntime = Substitute.For<IGrainRuntime>();
			_grainFactory = Substitute.For<IGrainFactory>();
			_grain = new MethodInvocationGrain("Derivco.Orniscient.Proxy.Tests.Grains.TestGrain", orleansIdentity, orleansRuntime, _grainFactory);
		}

		[Fact]
		public async Task GetAvailableMethods_TestGrain_ShouldReturnAllAvailableOrniscientMethods()
		{
			await _grain.OnActivateAsync();

			var response = await _grain.GetAvailableMethods();

			Assert.NotNull(response);
			Assert.True(response.Count == 2);
		}

		[Fact]
		public async Task GetGrainKeyType_TestGrain_ShouldReturnCorrectKeyType()
		{
			await _grain.OnActivateAsync();

			var response = await _grain.GetGrainKeyType();

			Assert.NotNull(response);
			Assert.Equal("System.String", response);
		}

		[Fact]
		public async Task InvokeGrainMethod_TestGrain_InvokesMethodOnGrain()
		{
			const string primaryKey = "Yoh";
			await _grain.OnActivateAsync();

			var testGrain = Substitute.For<ITestGrain>();
			var methods = await _grain.GetAvailableMethods();
			var method = methods[1];
			_grainFactory.GetGrain<ITestGrain>(primaryKey).Returns(testGrain);
			testGrain.TestOrniscientMethodTwo().ReturnsForAnyArgs(Task.FromResult("Great Success!"));

			var response = await _grain.InvokeGrainMethod(primaryKey, method.MethodId, "[]", true);

			await testGrain.ReceivedWithAnyArgs(1).TestOrniscientMethodTwo();
			Assert.NotNull(response);
			Assert.Equal("Great Success!", response);
		}

		[Fact]
		public async Task InvokeGrainMethod_TestGrain_InvokesMethodOnGrainWithParameters()
		{
			const string primaryKey = "Yoh";
			await _grain.OnActivateAsync();

			var testGrain = Substitute.For<ITestGrain>();
			var methods = await _grain.GetAvailableMethods();
			var method = methods[0];
			_grainFactory.GetGrain<ITestGrain>(primaryKey).Returns(testGrain);
			testGrain.TestOrniscientMethodOne(2).Returns(Task.FromResult(4));

			var response = await _grain.InvokeGrainMethod(primaryKey, method.MethodId, "[{'name': 'one', 'type': 'System.Int32', 'value': 2}]", true);

			await testGrain.Received(1).TestOrniscientMethodOne(2);
			Assert.NotNull(response);
			Assert.Equal(4, response);
		}
	}
}