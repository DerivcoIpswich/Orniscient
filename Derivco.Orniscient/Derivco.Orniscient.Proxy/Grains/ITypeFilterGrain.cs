using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface ITypeFilterGrain : IGrainWithStringKey
    {
        Task RegisterFilter(string typeName, string grainId, FilterRow[] filters);
    }
}
