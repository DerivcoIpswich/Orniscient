using System.Threading.Tasks;
using Orleans;

namespace TestHost.Grains
{
    public interface IFooGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }
}
