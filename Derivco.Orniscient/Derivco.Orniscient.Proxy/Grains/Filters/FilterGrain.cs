using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public class FilterGrain : Grain, IFilterGrain
    {
        public async Task<List<TypeFilter>> GetFilters(string[] types)
        {
            var result = new List<TypeFilter>();
            foreach (var type in types)
            {
                var typeFilterGrain = GrainFactory.GetGrain<ITypeFilterGrain>(type);
                var typeFilters = await typeFilterGrain.GetFilters();
                if (typeFilters.Count > 0)
                {
                    result.Add(new TypeFilter()
                    {
                        TypeName = type.Split('.').LastOrDefault(),
                        Filters = typeFilters
                    });
                }
            }
            return result;
        }

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }
    }
}
