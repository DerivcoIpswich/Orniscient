using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class TypeFilterGrain : Grain, ITypeFilterGrain
    {
        private List<FilterRow> _filters;
        private Logger _logger;

        public override Task OnActivateAsync()
        {
            _logger = GetLogger("TypeFilterGrain");
            _filters = new List<FilterRow>();
            RegisterTimer(SendFilters, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
            return base.OnActivateAsync();
        }

        private async Task SendFilters(object arg)
        {
            var filterGrain = GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
            await filterGrain.UpdateTypeFilters(this.GetPrimaryKeyString(), _filters);
            _filters.Clear();
        }

        public Task RegisterFilter(string typeName, string grainId, FilterRow[] filters)
        {
            //will push to local state, then the timer will flush the state
            _logger.Verbose($"Filters Registered for Grain[{typeName},Id:{grainId}][{string.Join(",", filters.Select(p => $"{p.FilterName} : {p.Value}"))}]");
            filters.All(p =>
            {
                p.GrainId = grainId;
                return true;
            });

            _filters.AddRange(filters);
            return TaskDone.Done;
        }
    }
}