using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public interface IFilterGrain : IGrainWithGuidKey
    {
        Task<List<TypeFilter>> GetFilters(string[] types);
        Task KeepAlive();
    }
}
