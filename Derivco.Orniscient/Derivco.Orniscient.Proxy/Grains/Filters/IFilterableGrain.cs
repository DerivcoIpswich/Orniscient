using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public interface IFilterableGrain : IGrainWithGuidKey 
    {
        Task<FilterRow[]> GetFilters();
    }
}
