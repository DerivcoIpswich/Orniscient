using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        Task KeepAlive();
    }

    public class TypeFilterGrain : Grain,ITypeFilterGrain
    {
        private List<FilterRow> _filters;

        public override Task OnActivateAsync()
        {
            _filters = new List<FilterRow>();
            RegisterTimer(UpdateFilters, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20));
            return base.OnActivateAsync();
        }

        private async Task UpdateFilters(object o)
        {
            //get all grains of this type.
            var orniscientReportingGrain = GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var grains = await orniscientReportingGrain.GetAll(this.GetPrimaryKeyString());

            foreach (var grain in grains)
            {
                var filterableGrain = GrainFactory.GetGrain<IFilterableGrain>(grain.Guid, grain.Type);
                var grainFilters = await filterableGrain.GetFilters();
                if(grainFilters==null)
                        continue;

                foreach (var grainFilter in grainFilters)
                {
                    if (!_filters.Any(p => p.Name==grainFilter.Name && p.Value==grainFilter.Value))
                    {
                        _filters.Add(grainFilter);
                    }
                }
            }
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
