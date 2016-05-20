using System.Threading.Tasks;
using Orleans;

namespace Derivco.Orniscient.TestImplementation.Grains
{
    public interface ISubGrain : IGrainWithGuidKey
    {
        Task SayHallo();
    }
}
