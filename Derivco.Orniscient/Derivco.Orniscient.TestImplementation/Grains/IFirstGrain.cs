using System.Threading.Tasks;
using Orleans;

namespace Derivco.Orniscient.TestImplementation.Grains
{
    public interface IFirstGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }
}
