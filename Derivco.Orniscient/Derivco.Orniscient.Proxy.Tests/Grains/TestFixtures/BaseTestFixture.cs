using Derivco.Orniscient.Proxy.BootstrapProviders;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;

namespace Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures
{
    public class BaseTestFixture
    {
        public BaseTestFixture()
        {
            var options = new TestClusterOptions(1);
            options.ClusterConfiguration.Globals.RegisterBootstrapProvider<OrniscientFilterInterceptor>("OrniscientFilterInterceptor");
            //options.ClusterConfiguration.Globals.ResponseTimeout = TimeSpan.FromMinutes(1);
            options.ClusterConfiguration.ApplyToAllNodes(nodeConfig => nodeConfig.MaxActiveThreads = 1);
            this.HostedCluster = new TestCluster(options);
            if (this.HostedCluster.Primary == null)
            {
                HostedCluster.Deploy();
            }
        }

        public void Dispose()
        {
            this.HostedCluster.StopAllSilos();
        }

        public TestCluster HostedCluster { get; private set; }
    }
}
