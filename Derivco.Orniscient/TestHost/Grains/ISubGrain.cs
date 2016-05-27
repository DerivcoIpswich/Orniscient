using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;

namespace TestHost.Grains
{
    public interface ISubGrain : IFilterableGrain
    {
        Task SayHallo();
    }
}
