using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;

namespace TestGrains.Grains
{
    public interface ISubGrain : IGrainWithGuidKey,IFilterable
    {
        Task SayHallo();
    }
}
