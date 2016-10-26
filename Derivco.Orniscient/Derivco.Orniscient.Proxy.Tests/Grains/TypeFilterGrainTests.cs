using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Tests.Utils;
using NSubstitute;
using Orleans.Core;
using Xunit;


namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class TypeFilterGrainTests 
    {
        [Fact]
        public async Task RegisterFilter_ValidParams_ShouldAddFilterToList()
        {
            var grainIdentity = Substitute.For<IGrainIdentity>();
            var grain = new TypeFilterGrain(grainIdentity, TestHelpers.MockRuntime());
            await grain.RegisterFilter("TestType", "TestGrainId", new[] { new FilterRow("FilterName", "FilterValue") });
            Assert.NotEmpty(grain.Filters);
        }

        [Fact]
        public async Task SendFilters_ShouldSendFiltersToFilterGrainAndClearField()
        {
            var grainIdentity = Substitute.For<IGrainIdentity>();
            var filterList = new List<FilterRow>() { new FilterRow("FilterTest", "FilterValue") };
            var filterGrain = Substitute.For<IFilterGrain>();
            var grainRunTime = TestHelpers.MockRuntime();
            grainRunTime.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty).Returns(p => filterGrain);

            var grain = new TypeFilterGrain(grainIdentity, grainRunTime)
            {
                Filters =filterList
            };

            await grain.SendFilters(null);

            await filterGrain.Received().UpdateTypeFilters(Arg.Any<string>(), filterList);
            Assert.Empty(grain.Filters);
        }
    }
}
