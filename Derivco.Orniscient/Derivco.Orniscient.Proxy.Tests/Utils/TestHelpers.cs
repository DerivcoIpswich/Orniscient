using System;
using Derivco.Orniscient.Proxy.Grains.Models;
using NSubstitute;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.TestingHost.Utils;

namespace Derivco.Orniscient.Proxy.Tests.Utils
{
    public class TestHelpers
    {
        public static IGrainRuntime MockRuntime()
        {
            var grainRuntime = Substitute.For<IGrainRuntime>();
            grainRuntime.GetLogger(Arg.Any<string>()).Returns(new NoOpTestLogger());
            return grainRuntime;
        }
    }

    public static class TestHelperExtensions
    {
        /// <summary>
        /// Use this to inject a mock stream into the runtime.
        /// </summary>
        /// <typeparam name="T">The type parameter for the stream</typeparam>
        /// <param name="runtime">The runtime to inject the stream mock into</param>
        /// <param name="returnStream">The stream to return</param>
        public static void MockStream<T>(this IGrainRuntime runtime, IAsyncStream<T> returnStream)
        {
            //streams
            var streamProvider = Substitute.For<IStreamProvider, IProvider>();
            var streamProviderManager = NSubstitute.Substitute.For<IStreamProviderManager>();

            streamProvider.GetStream<T>(Guid.Empty, Arg.Any<string>()).Returns(returnStream);

            streamProviderManager.GetProvider(Arg.Any<string>()).Returns(streamProvider as IProvider);
            runtime.StreamProviderManager.Returns(streamProviderManager);
        }

        
    }
}
