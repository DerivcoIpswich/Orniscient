using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Tests.Utils;
using NSubstitute;
using Orleans.Core;
using Orleans.Providers.Streams.SimpleMessageStream;
using Orleans.Storage;
using Orleans.TestingHost;
using Xunit;

namespace Derivco.Orniscient.Proxy.Tests.Grains
{
    public class DashboardInstanceGrainTests
    {
        public DashboardInstanceGrainTests()
        {
            
        }

        [Fact]
        public async Task GetAll_NoFilter_ShouldReturnAllGrainsFromDashboardCollector()
        {

            var dashboardCollectedGrain = NSubstitute.Substitute.For<IDashboardCollectorGrain>();
            dashboardCollectedGrain.GetAll().Returns(new List<UpdateModel>()
            {

            });

            
            var runtime = TestHelpers.MockRuntime();
            runtime.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty).Returns(dashboardCollectedGrain);





            var grain = new DashboardInstanceGrain(Substitute.For<IGrainIdentity>(), runtime);
            await grain.GetAll(null);
        }




    }









    public class TestClass : TestCluster
    {
        public TestClass() 
        {
            //how can we add the config from the files. That will be awesome..
            this.ClusterConfiguration.Globals.RegisterStreamProvider<SimpleMessageStreamProvider>("SMSProvider", new Dictionary<string, string> { { "FireAndForgetDelivery", "false"} });
            this.ClusterConfiguration.Globals.RegisterStorageProvider<MemoryStorage>("Default");
            this.ClusterConfiguration.Globals.RegisterStorageProvider<MemoryStorage>("PubSubStore");
            this.ClusterConfiguration.Globals.RegisterStorageProvider<MemoryStorage>("MemoryStore");
            Deploy();
        }

        [Fact]
        public async Task GetAll_ShouldReturnTheGrainsThatWasFilteredUsingApplyFilter()
        {
            var grain = GrainFactory.GetGrain<IDashboardInstanceGrain>(1);
            var result = await grain.GetAll();
            Assert.NotEmpty(result.NewGrains);
        }
    }
}
