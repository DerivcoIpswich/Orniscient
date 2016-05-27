using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public interface IFilterGrain : IGrainWithStringKey
    {
        Task<FilterRow[]> GetFilters();
        Task UpdateModels();
    }
}
