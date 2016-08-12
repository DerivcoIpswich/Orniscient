using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Tests.Utils;
using NSubstitute;
using Orleans.Core;
using Orleans.Runtime;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class DashboardInstanceGrainTests
    {
        #region GetAll

        [Fact]
        public async Task GetAll_NoFilter_ShouldReturnAllGrainsFromDashboardCollector()
        {
            var dashboardCollectedGrain = NSubstitute.Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            var result = await grain.GetAll(null);

            Assert.NotEmpty(result.NewGrains);
        }

        [Fact]
        public async Task GetAll_FilterWithGrainIdPassed_ShouldReturnGrainWithThatId()
        {
            var dashboardCollectedGrain = Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            var result = await grain.GetAll(new AppliedFilter()
            {
                GrainId = "1"
            });

            Assert.NotEmpty(result.NewGrains);
            Assert.Equal(true, result.NewGrains.TrueForAll(g => g.GrainId.Contains("1")));
        }

        [Fact]
        public async Task GetAll_FilterWithSiloPassed_ShouldFilterOutThoseSilos()
        {
            var dashboardCollectedGrain = Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            var result = await grain.GetAll(new AppliedFilter()
            {
                SelectedSilos = new [] { "Silo1"}
            });

            Assert.NotEmpty(result.NewGrains);
            Assert.Equal(true, result.NewGrains.TrueForAll(g => g.Silo.Contains("Silo1")));
        }

        [Fact]
        public async Task GetAll_FilterWithTypeFilterOnly_ShouldReturnGrainsWithThatTypeOnly()
        {
            var dashboardCollectedGrain = Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            var result = await grain.GetAll(new AppliedFilter()
            {
                TypeFilters = new List<AppliedTypeFilter>()
                {
                    new AppliedTypeFilter() {TypeName= "GrainType1"}
                }
            });

            Assert.NotEmpty(result.NewGrains);
            Assert.Equal(true, result.NewGrains.TrueForAll(g => g.Type.Contains("GrainType1")));
        }

        [Fact]
        public async Task GetAll_FilterWithFilterTypeAndSelectedValues_ShouldReturnGrainsWithThoseTypesAndValues()
        {
            var dashboardCollectedGrain = Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());


            //Mock the filterGrain
            var filterGrain = Substitute.For<IFilterGrain>();
            filterGrain.GetFilters(Arg.Any<string[]>()).Returns(new List<TypeFilter>()
            {
                new TypeFilter()
                {
                    TypeName = "GrainType1",
                    Filters = new List<FilterRow>()
                    {
                        new FilterRow()
                        {
                            FilterName = "Filter1",
                            Value = "GrainId1Filter",
                            GrainId = "1"
                        },
                        new FilterRow()
                        {
                            FilterName = "Filter1",
                            Value = "GrainId3Filter",
                            GrainId = "3"
                        }
                    }
                }
            });

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);
            runtime.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty).Returns(filterGrain);


            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            var result = await grain.GetAll(new AppliedFilter()
            {
                TypeFilters = new List<AppliedTypeFilter>()
                {
                    new AppliedTypeFilter()
                    {
                        TypeName = "GrainType1",
                        SelectedValues = new Dictionary<string, IEnumerable<string>> {{ "Filter1", new[] { "GrainId1Filter" } }}
                    }
                }
            });

            Assert.NotEmpty(result.NewGrains);
            Assert.Equal(true, result.NewGrains.TrueForAll(g => g.GrainId=="1"));
        }

        [Fact]
        public async Task GetAll_WhenOverSummaryViewLimit_ShouldReturnDiffModelWithSummaryViewTrue()
        {
            var dashboardCollectedGrain = NSubstitute.Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.SetSummaryViewLimit(1);
            var result = await grain.GetAll(null);

            Assert.True(result.SummaryView);
        }

        [Fact]
        public async Task GetAll_WhenInSummaryView_ShouldReturnDistinctTypesWithCounts()
        {
            var dashboardCollectedGrain = NSubstitute.Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(GetAllMock());

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.SetSummaryViewLimit(1);
            var result = await grain.GetAll(null);
            Assert.Equal(2, result.NewGrains.Count);
        }

        private List<UpdateModel> GetAllMock()
        {
            return new List<UpdateModel>()
            {
                new UpdateModel()
                {
                    GrainId = "1",
                    Type = "GrainType1",
                    Silo = "Silo1"
                },
                new UpdateModel()
                {
                    GrainId = "3",
                    Type = "GrainType1",
                    Silo = "Silo1"
                },
                new UpdateModel()
                {
                    GrainId = "2",
                    Type = "GrainType2",
                    Silo = "Silo2"
                },
                new UpdateModel()
                {
                    GrainId = "4",
                    Type = "GrainType2",
                    Silo = "Silo2"
                }
            };
        }

        #endregion

        #region GetGrainTypes

        [Fact]
        public async Task GetGrainTypes_ShouldReturnGrainTypesFromDashboardCollectorGrain()
        {
            var dashboardCollectedGrain = Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetGrainTypes().Returns(new GrainType[] {new GrainType("TestType")});

            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);

            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(),runtime);
            var result = await grain.GetGrainTypes();

            Assert.NotEmpty(result);
        }

        #endregion
    }
}
