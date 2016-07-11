using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public interface IFilterGrain : IGrainWithGuidKey
    {
        Task<List<TypeFilter>> GetFilters(string[] types);
        Task<List<GroupedTypeFilter>> GetGroupedFilterValues(string[] types);
        Task<TypeFilter> GetFilter(string type);
        Task KeepAlive();
        Task<List<FilterRow>> GetFilters(string type, string grainId);
        Task UpdateTypeFilters(string typeName, IEnumerable<FilterRow> filters);
    }
}
