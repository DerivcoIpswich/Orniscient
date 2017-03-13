using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IGrainInfoGrain : IGrainWithStringKey
    {
        Task<List<GrainMethod>> GetAvailableMethods();
        Task<string> GetGrainKeyType();
        Task<object> InvokeGrainMethod(string id, string methodId, string parametersJson, bool invokeOnNewGrain = false);
    }
}
