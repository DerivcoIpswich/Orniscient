using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains.Filters
{
    public class FilterGrain : Grain, IFilterGrain
    {
        private FilterRow[] _filters;

        public override Task OnActivateAsync()
        {
            if (_filters == null)
            {
                _filters = new FilterRow[0];
            }
            RegisterTimer(UpdateFilters, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15));
            return TaskDone.Done;
        }

        public Task<FilterRow[]> GetFilters()
        {
            return Task.FromResult(_filters);
        }

        public Task UpdateModels()
        {
            return TaskDone.Done;
        }

        private async Task UpdateFilters(object state = null)
        {
            var filters = new List<FilterRow>();

            var reportingGrain = GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var models = await reportingGrain.GetAll(this.GetPrimaryKeyString());

            foreach (var model in models)
            {
                //TODO : Makre sure there are no duplicates in the list here....
                try
                {
                    var filterGrain = GrainFactory.GetGrain<IFilterableGrain>(model.Guid, model.Type);
                    if (filterGrain == null)
                    {
                        Debug.WriteLine("This thing is null");
                        continue;
                    }
                    var grainFilters = await filterGrain.GetFilters();
                    if (grainFilters != null)
                    {
                        filters.AddRange(grainFilters);
                    }
                }
                catch (Exception)
                {

                    throw;
                }


                // TODO: concat efficiently.

                //var type = Type.GetType(model.Type, false);
                //var getGrainGeneric = getGrainMethod.MakeGenericMethod(type);
                //var grain = (IFilterableGrain)getGrainGeneric.Invoke(GrainFactory, new object[] { model.Guid });
                //var grainFilters = await grain.GetFilters();
                //filters.AddRange(grainFilters);
            }


            //var getGrainMethod = GrainFactory.GetType().GetMethod("GetGrain");
            _filters = filters.ToArray();
        }
    }
}
