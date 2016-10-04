using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;
using Orleans.TestingHost;
using OrleansTelemetryConsumers.Counters;

namespace Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures
{
    public class BaseTestFixture
    {
        public BaseTestFixture()
        {
            
            var options = new TestClusterOptions(1);
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
