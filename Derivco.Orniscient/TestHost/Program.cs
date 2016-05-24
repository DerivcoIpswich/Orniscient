using System;
using Orleans;
using IFirstGrain = TestHost.Grains.IFirstGrain;

namespace TestHost
{
    class Program
    {
        private static OrleansHostWrapper hostWrapper;

        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            var hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args
            });

            GrainClient.Initialize("DevTestClientConfiguration.xml");
            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");
            

            //Now we need some test classes....
            var firstGrain = GrainClient.GrainFactory.GetGrain<IFirstGrain>(Guid.Empty);
            firstGrain.KeepAlive();

            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }

        private static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        private static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }
    }
}
