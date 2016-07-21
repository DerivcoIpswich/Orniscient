using System.Threading.Tasks;
using Orleans;

namespace TestGrains.Grains
{
    public interface IFirstGrain : IGrainWithStringKey
    {
        Task KeepAlive();
    }
}
