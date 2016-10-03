using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures;
using Orleans.Runtime;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class TypeMethodGrainTests : IClassFixture<TypeMethodGrainTestFixture>
    {
        private readonly TypeMethodGrainTestFixture _fixture;

        public TypeMethodGrainTests(TypeMethodGrainTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAvailableMethods_FooGrain_ShouldReturnSixMethods()
        {
            const int expected = 6;
            var methodGrain = _fixture.HostedCluster.GrainFactory.GetGrain<ITypeMethodsGrain>("TestGrains.Grains.FooGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }

        [Fact]
        public async Task GetAvailableMethods_FirstGrain_ShouldReturnZeroMethods()
        {
            const int expected = 0;
            var methodGrain = _fixture.HostedCluster.GrainFactory.GetGrain<ITypeMethodsGrain>("TestGrains.Grains.FirstGrain");

            var reply = await methodGrain.GetAvailableMethods();
            
            Assert.NotNull(reply);
            Assert.Equal(expected, reply.Count);
        }
    }
}
