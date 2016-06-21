using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    /// <summary>
    /// This grain will collect all the filters for a specific type. 
    /// </summary>
    public interface ITypeFilterGrain : IGrainWithStringKey
    {
        Task<List<AggregatedFilterRow>> GetFilters();
        Task<List<string>> GetGrainIdsForFilter(AppliedTypeFilter typeFilter);
        Task<List<FilterRow>> Getfilters(string grainId);
        Task KeepAlive();
    }
}
