using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public class TypeFilterGrain : Grain,ITypeFilterGrain
    {
        private List<FilterRowSummary> _filters;

        public override Task OnActivateAsync()
        {
            _filters = new List<FilterRowSummary>();
            RegisterTimer(UpdateFilters, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20));
            return base.OnActivateAsync();
        }

        private async Task UpdateFilters(object o)
        {
            //get all grains of this type.
            var orniscientReportingGrain = GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var grains = await orniscientReportingGrain.GetAll(this.GetPrimaryKeyString());

            var getFilterTasks = grains.Select(async model =>
            {
                var filterableGrain = GrainFactory.GetGrain<IFilterableGrain>(model.Guid, model.Type);
                if (filterableGrain == null)
                    return TaskDone.Done;
                var filters = await filterableGrain?.GetFilters();
                foreach (var row in filters)
                {
                    var filterRow = _filters.FirstOrDefault(p => p.Name == row.Name && p.Value == row.Value);
                    if (filterRow == null)
                    {
                        _filters.Add(new FilterRowSummary(row, model.Guid.ToString()));
                    }
                    else
                    {
                        filterRow.GrainsWithValue.Add(model.Guid.ToString());    
                    }
                }
                return TaskDone.Done;
            });

            await Task.WhenAll(getFilterTasks);
            Debug.WriteLine($"Finished updating filter for ITypeFilter<{this.GetPrimaryKeyString()}>");
        }

        public Task<List<AggregatedFilterRow>> GetFilters()
        {
            if (_filters == null)
                return null;

            var result = _filters.GroupBy(p => p.Name).Select(p => new AggregatedFilterRow()
            {
                FilterName = p.Key,
                Type = this.GetPrimaryKeyString(),
                Values = _filters.Where(f => f.Name == p.Key).Select(f=>f.Value).ToList()
            }).ToList();

            return Task.FromResult(result);
        }

        public Task KeepAlive()
        {
            Debug.WriteLine($"....KeepAlive for ITypeFilter<{this.GetPrimaryKeyString()}> called.");
            return TaskDone.Done;
        }
    }
}