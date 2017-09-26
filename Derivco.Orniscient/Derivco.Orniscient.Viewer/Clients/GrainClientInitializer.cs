using Derivco.Orniscient.Proxy;
using Orleans;
using Orleans.Runtime.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Derivco.Orniscient.Viewer.Clients
{
    public static class GrainClientInitializer
    {
        private static object _lock = new object();
        private static bool _initCalled = false;

        public static Task InitializeIfRequired(string path)
        {

            lock (_lock)
            {
                if (_initCalled || GrainClient.IsInitialized)
                    return TaskDone.Done;
                _initCalled = true;

                GrainClient.Initialize(path);
                return TaskDone.Done;
            }

        }

        public static Task InitializeIfRequired(string address, int port)
        {
            lock (_lock)
            {
                var host = Dns.GetHostEntry(address);
                var ipAddress = host.AddressList.Last();
                var ipEndpoint = new IPEndPoint(ipAddress, port);
                var config = new ClientConfiguration();
                config.Gateways = new List<IPEndPoint>();
                config.Gateways.Add(ipEndpoint);

                config.RegisterStreamProvider("Orleans.Providers.Streams.SimpleMessageStream.SimpleMessageStreamProvider", StreamKeys.StreamProvider);
                GrainClient.Initialize(config);
            }
            return TaskDone.Done;
        }

        public static void Disconnect()
        {
            _initCalled = false;
            GrainClient.Uninitialize();
        }
    }
}
