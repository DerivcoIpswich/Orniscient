using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    /// <summary>
    /// Add this to a grain to give it the opportunity to return filter values for orniscient.
    /// </summary>
    public interface IFilterable 
    {
        Task<FilterRow[]> GetFilters();
    }
}
