using System.Threading.Tasks;
using Orleans;

namespace TestHost.Grains
{
    public interface ISubGrain : IGrainWithGuidKey
    {
        Task SayHallo();
    }
}
