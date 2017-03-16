using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures;
using Orleans;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class TypeMethodGrainTests : IClassFixture<TypeMethodGrainTestFixture>
    {
        protected static IGrainFactory GrainFactory => GrainClient.GrainFactory;

        [Fact]
        public async Task GetAvailableMethods_FooGrain_ShouldReturnSixMethods()
        {
            const int expected = 6;
            var methodGrain = GrainFactory.GetGrain<IMethodInvocationGrain>("TestGrains.Grains.FooGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }

        [Fact]
        public async Task GetAvailableMethods_FirstGrain_ShouldReturnOneMethod()
        {
            const int expected = 1;
            var methodGrain = GrainFactory.GetGrain<IMethodInvocationGrain>("TestGrains.Grains.FirstGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }
    }
}
