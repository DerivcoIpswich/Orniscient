using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace TestHost.Grains
{
    public interface IFooGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }
}
