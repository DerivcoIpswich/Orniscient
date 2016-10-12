using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Derivco.Orniscient.Viewer.Observers;
using Orleans;
using System.Configuration;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Derivco.Orniscient.Viewer.Clients
{
    public static class GrainClientInitializer
    {
        private static object _lock = new object();

        public static async Task InitializeIfRequired(string path)
        {
            lock (_lock)
            {
                if (GrainClient.IsInitialized)
                    return;
                GrainClient.Initialize(path);
                GrainClient.Logger.Info("Orniscient Grain Client Initialized");
            }
            await OrniscientObserver.Instance.SetTypeFilter(p => p.FullName.Contains(ConfigurationManager.AppSettings["GlobalFilter"]));
        }
    }
}