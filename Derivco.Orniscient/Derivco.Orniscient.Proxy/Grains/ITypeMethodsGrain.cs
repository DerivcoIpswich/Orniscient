using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface ITypeMethodsGrain : IGrainWithStringKey
    {
        Task<List<GrainMethod>> GetAvailableMethods();

        //TODO : this needs to be defined
        //Task InvokeMethod(string name);
    }
}
