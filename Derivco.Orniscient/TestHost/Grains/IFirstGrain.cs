using System.Threading.Tasks;
using Orleans;

namespace TestHost.Grains
{
    public interface IFirstGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }
}
