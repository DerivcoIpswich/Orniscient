using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public class FilterGrain : Grain, IFilterGrain
    {
        private List<TypeFilter> _filters;
        private Logger _logger;

        public override async  Task OnActivateAsync()
        {
            _logger = GetLogger("FilterGrain");
            _filters = new List<TypeFilter>();
            await base.OnActivateAsync();
        }

        public Task<List<TypeFilter>> GetFilters(string[] types)
        {
            return Task.FromResult(_filters);
        }

        public Task<List<GroupedTypeFilter>>  GetGroupedFilterValues(string[] types)
        {
            var result = new List<GroupedTypeFilter>();

            foreach (var type in types)
            {
                var currentTypeFilter = _filters.FirstOrDefault(p => p.TypeName == type);
                if (currentTypeFilter != null)
                {
                    result.Add(new GroupedTypeFilter()
                    {
                        TypeName = type,
                        Filters = currentTypeFilter.Filters
                            .GroupBy(p => p.FilterName)
                            .Select(g => new GroupedFilter()
                            {
                                FilterName = g.Key,
                                Values = currentTypeFilter.Filters.Where(f => f.FilterName == g.Key).Select(s => s.Value).Distinct().ToList()
                            }).ToList()
                    });
                }
            }
            return Task.FromResult(result);
        }

        public Task<TypeFilter> GetFilter(string type)
        {
            return Task.FromResult(_filters.FirstOrDefault(p => p.TypeName == type));
        }

        public Task KeepAlive()
        {
            return TaskDone.Done;
        }

        public Task<List<FilterRow>> GetFilters(string type, string grainId)
        {
            var typefilter = _filters.FirstOrDefault(p => p.TypeName == type);
            return typefilter != null ? Task.FromResult(typefilter.Filters.Where(p => p.GrainId == grainId).ToList()) : Task.FromResult<List<FilterRow>>(null);
        }

        public Task RegisterFilter(string typeName, string grainId, FilterRow[] filters)
        {
            _logger.Verbose($"Filters Registered for Grain[{typeName},Id:{grainId}][{string.Join(",", filters.Select(p => $"{p.FilterName} : {p.Value}"))}]");
            filters.All(p =>
            {
                p.GrainId = grainId;
                return true;
            });

            var typeFilter = _filters.FirstOrDefault(p => p.TypeName == typeName);
            if (typeFilter != null)
            {
                typeFilter.Filters.AddRange(filters);
            }
            else
            {
                _filters.Add(new TypeFilter() {TypeName = typeName,Filters = filters.ToList()});
            }
            return TaskDone.Done;
        }
    }
}
