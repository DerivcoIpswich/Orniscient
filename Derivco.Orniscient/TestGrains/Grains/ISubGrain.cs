using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Filters;

namespace TestGrains.Grains
{
    public interface ISubGrain : IFilterableGrain
    {
        Task SayHallo();
    }
}
