using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Filters;

namespace TestHost.Grains
{
    public interface ISubGrain : IFilterableGrain
    {
        Task SayHallo();
    }
}
