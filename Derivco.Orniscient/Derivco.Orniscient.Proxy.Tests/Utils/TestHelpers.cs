using NSubstitute;
using Orleans.Runtime;
using Orleans.TestingHost.Utils;

namespace Derivco.Orniscient.Proxy.Tests.Utils
{
    public class TestHelpers
    {
        public static IGrainRuntime MockRuntime()
        {
            var grainRunTime = Substitute.For<IGrainRuntime>();
            grainRunTime.GetLogger(Arg.Any<string>()).Returns(new NoOpTestLogger());
            return grainRunTime;
        }
    }
}
