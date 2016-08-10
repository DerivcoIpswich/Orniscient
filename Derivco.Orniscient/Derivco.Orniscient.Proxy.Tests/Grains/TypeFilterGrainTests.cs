using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Orleans;
using Orleans.TestingHost;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class TypeFilterGrainTests : TestCluster
    {
        public TypeFilterGrainTests()
        {
            this.Deploy();
        }

        [Fact]
        public async Task RegisterFilter_ValidParams_ShouldAddFilterToList()
        {
            var grain = this.GrainFactory.GetGrain<ITypeFilterGrain>("TestTypeString");
            await grain.RegisterFilter("TestType", "TestGrainId", new[] {new FilterRow("FilterName", "FilterValue")});
            var grainimpl = grain.AsReference<TypeFilterGrain>();
            var temp = grainimpl.Filters.Count;

            
        }
    }
}
