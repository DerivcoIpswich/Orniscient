using Orleans;
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
    }
}
