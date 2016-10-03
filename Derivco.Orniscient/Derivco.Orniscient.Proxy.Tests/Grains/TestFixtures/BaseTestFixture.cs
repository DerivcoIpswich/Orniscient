using Orleans.TestingHost;

namespace Derivco.Orniscient.Proxy.Tests.Grains.TestFixtures
{
    public class BaseTestFixture
    {
        public BaseTestFixture()
        {
            this.HostedCluster = new TestCluster();
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
