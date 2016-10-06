using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Tests.Utils;
using Orleans.Core;
using Xunit;
using NSubstitute;

namespace Derivco.Orniscient.Proxy.Tests.Grains.Filters
{
    public class FilterGrainTests
    {
        [Fact]
        public async Task UpdateTypeFilters_FiltersThatWasPassedNeedsToBeAddedToTheGrainsFilterList()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(),runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType", new List<FilterRow>()
            {
                new FilterRow("Filter1", "Filter 1 Value")
            });

            var typeFilter  = await grain.GetFilter("TestType");
            Assert.NotEmpty(typeFilter.Filters);
            Assert.Equal(true, typeFilter.Filters.TrueForAll(g => g.FilterName.Equals("Filter1")));
        }

        [Fact]
        public async Task UpdateTypeFilters_IfThereIsFiltersOfPassedType_ShouldAddToThatFilterNotCreateaNewFilterForList()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType", new List<FilterRow>()
            {
                new FilterRow("Filter1", "Filter 1 Value")
            });

            await grain.UpdateTypeFilters("TestType", new List<FilterRow>()
            {
                new FilterRow("Filter2", "Filter 2 Value")
            });

            var filterForType = await grain.GetFilters(new [] { "TestType"});
            Assert.NotEmpty(filterForType);
            Assert.Single(filterForType);
        }

        [Fact]
        public async Task UpdateTypeFilters_WhenFilterAlreadyInTypeFilterList_ShouldUpdateTheValue()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType", new List<FilterRow>()
            {
                new FilterRow("Filter1", "Filter 1 Value")
            });

            await grain.UpdateTypeFilters("TestType", new List<FilterRow>()
            {
                new FilterRow("Filter1", "Filter 1 NewValue")
            });

            var filterForType = await grain.GetFilters(new[] { "TestType" });
            Assert.NotEmpty(filterForType);
            Assert.Single(filterForType);
            Assert.True(filterForType.First().Filters.First().Value== "Filter 1 NewValue");
        }

        [Fact]
        public async Task GetFilters_ShouldReturnAllFiltersFromGrain()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType1", new List<FilterRow>()
            {
                new FilterRow("TestType1_Filter1", "Filter 1 Value"),
                new FilterRow("TestType1_Filter2", "Filter 2 Value")
            });

            await grain.UpdateTypeFilters("TestType2", new List<FilterRow>()
            {
                new FilterRow("TestType2_Filter1", "Filter 1 Value") ,
                new FilterRow("TestType2_Filter2", "Filter 2 Value")
            });

            var filters= await grain.GetFilters(new []{"TestType1", "TestType2" });
            Assert.NotEmpty(filters);
            Assert.True(filters.Count == 2);
        }

        [Fact]
        public async Task GetFilters_ShouldReturnFiltersOfTypeAndGrainIdOnly()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType1", new List<FilterRow>()
            {
                new FilterRow("TestType1_Filter1", "Filter 1 Value") {GrainId = "1"},
                new FilterRow("TestType1_Filter2", "Filter 2 Value") {GrainId = "2"}
            });

            await grain.UpdateTypeFilters("TestType2", new List<FilterRow>()
            {
                new FilterRow("TestType2_Filter1", "Filter 1 Value"){GrainId = "3"} ,
                new FilterRow("TestType2_Filter2", "Filter 2 Value"){GrainId = "4"}
            });

            var filterRows = await grain.GetFilters("TestType1", "1");
            Assert.NotEmpty(filterRows);
            Assert.Single(filterRows);
        }

        [Fact]
        public async Task GetGroupedFilterValues_ShouldAllTheFiltersWithTheSameValues()
        {
            var runtime = TestHelpers.MockRuntime();
            var grain = new FilterGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.OnActivateAsync();

            await grain.UpdateTypeFilters("TestType1", new List<FilterRow>()
            {
                new FilterRow("TestType1_Filter1", "Filter 1 Value") {GrainId = "1"},
                new FilterRow("TestType1_Filter2", "Filter 2 Value") {GrainId = "2"}
            });

            await grain.UpdateTypeFilters("TestType1", new List<FilterRow>()
            {
                new FilterRow("TestType1_Filter1", "Filter 1 Value"){GrainId = "3"} ,
            });

            var filterRows = await grain.GetGroupedFilterValues(new[] {"TestType1"});
            
            Assert.NotEmpty(filterRows);
            Assert.Single(filterRows);
            Assert.True(filterRows.First().Filters.Count==2);
        }


    }
}
