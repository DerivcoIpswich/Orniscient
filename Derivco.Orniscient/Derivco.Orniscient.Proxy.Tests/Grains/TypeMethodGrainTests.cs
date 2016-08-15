using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Orleans.Runtime;
using Orleans.TestingHost;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class TypeMethodGrainTests : TestCluster
    {
        public TypeMethodGrainTests()
        {
            Deploy();
        }

        [Fact]
        public async Task GetAvailableMethods_FooGrain_ShouldReturnSixMethods()
        {
            const int expected = 6;
            var methodGrain = GrainFactory.GetGrain<ITypeMethodsGrain>("TestGrains.Grains.FooGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }

        [Fact]
        public async Task GetAvailableMethods_FirstGrain_ShouldReturnZeroMethods()
        {
            const int expected = 0;
            var methodGrain = GrainFactory.GetGrain<ITypeMethodsGrain>("TestGrains.Grains.FirstGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }

        [Fact]
        public async Task GetAvailableMethods_ForAnInvalidGrain_ShouldThrowOrleansException()
        {
            var methodGrain = GrainFactory.GetGrain<ITypeMethodsGrain>("Test");

            await Assert.ThrowsAsync<OrleansException>(async () => await methodGrain.GetAvailableMethods());
        }
    }
}
