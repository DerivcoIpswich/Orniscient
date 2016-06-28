using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IDashboardInstanceGrain : IGrainWithIntegerKey
    {
        Task<List<UpdateModel>> GetAll(AppliedFilter filter = null);
        Task Subscribe(IOrniscientObserver observer);
        Task UnSubscribe(IOrniscientObserver observer);
        Task SetTypeFilter(GrainType[] types);
        Task<GrainType[]> GetGrainTypes();
    }



    public class DashboardInstanceGrain : Grain, IDashboardInstanceGrain, IAsyncObserver<DiffModel>
    {
        private List<UpdateModel> CurrentStats { get; set; }
        private ObserverSubscriptionManager<IOrniscientObserver> _subsManager;
        private IDashboardCollectorGrain _dashboardCollectorGrain;
        private AppliedFilter _currentFilter;
        //private Logger _logger;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            //_logger = GetLogger();

            _dashboardCollectorGrain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
            _subsManager = new ObserverSubscriptionManager<IOrniscientObserver>();

            var streamProvider = GetStreamProvider(StreamKeys.StreamProvider);
            var stream = streamProvider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientChanges);
            await stream.SubscribeAsync<DiffModel>(OnNextAsync);

            //_logger.Info("DashboardInstanceGrain Activated.");
        }

        public async Task<List<UpdateModel>> GetAll(AppliedFilter filter = null)
        {
            _currentFilter = filter;

            var allGrains = await _dashboardCollectorGrain.GetAll();
            return await ApplyFilter(allGrains);
        }

        public Task Subscribe(IOrniscientObserver observer)
        {
            _subsManager.Subscribe(observer);
            return TaskDone.Done;
        }

        public Task UnSubscribe(IOrniscientObserver observer)
        {
            _subsManager.Unsubscribe(observer);
            return TaskDone.Done;
        }

        public Task SetTypeFilter(GrainType[] types)
        {
            //TODO : Implement the SetTypeFilter on this level
            return TaskDone.Done;
        }

        public Task<GrainType[]> GetGrainTypes()
        {
            return _dashboardCollectorGrain.GetGrainTypes();
        }

        private async Task<List<UpdateModel>> ApplyFilter(List<UpdateModel> grains)
        {
            //_logger.Verbose("Filter Applied");
            if (_currentFilter == null)
                return grains;

            //order of filtering applies here.
            //1. Grain Id
            if (!string.IsNullOrEmpty(_currentFilter.GrainId))
            {
                return grains.Where(p => p.Guid.ToString().Contains(_currentFilter.GrainId)).ToList();
            }

            //2. Silo
            var grainQuery = grains.Where(p => _currentFilter.SelectedSilos == null || _currentFilter.SelectedSilos.Length == 0 || _currentFilter.SelectedSilos.Contains(p.Silo));

            //3. Apply Type Filters

            if (_currentFilter.TypeFilters != null && _currentFilter.TypeFilters.Any())
            {
                //TODO : we need to dynamically build up the expression tree here

                grainQuery = grains.Where(p => _currentFilter.TypeFilters.Any(t => t.TypeName == p.Type));

                var grainIdsToFilter = new List<string>();
                foreach (var appliedTypeFilter in _currentFilter.TypeFilters)
                {
                    if (appliedTypeFilter.SelectedValues != null && appliedTypeFilter.SelectedValues.Any())
                    {
                        var typeFilterGrain = GrainFactory.GetGrain<ITypeFilterGrain>(appliedTypeFilter.TypeName);

                        var grainsToFilter = await typeFilterGrain.GetGrainIdsForFilter(appliedTypeFilter).ConfigureAwait(false);
                        //_logger.Info($"GetGrainIdsForFilter Returning {grainsToFilter.Count} items");
                        grainIdsToFilter.AddRange(grainsToFilter);
                    }
                }



                //var managementGrain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
                //var allGrains = await managementGrain.GetAll();
                //grainIdsToFilter = allGrains.Select(p => p.Guid.ToString()).ToList();
                //Debug.WriteLine("So all grains in system is :" + grainIdsToFilter.Count);

                if (grainIdsToFilter.Any())
                {
                    //var items = (from g in grainQuery.ToList()
                    //    join grainfilters in grainIdsToFilter on g.Guid.ToString() equals grainfilters
                    //    select g).ToList();
                    var items = new List<UpdateModel>();

                    foreach (var updateModel in grainQuery.ToList())
                    {
                        if (grainIdsToFilter.Contains(updateModel.Guid.ToString()))
                        {
                            Debug.WriteLine(updateModel.Guid.ToString());
                            items.Add(updateModel);
                        }
                    }
                    //grainQuery = grainQuery.Where(p => grainIdsToFilter.Contains(p.Guid.ToString())).ToList(); // grainIdsToFilter.Any(g => g == p.Guid.ToString()))).ToList();
                    Debug.WriteLine($"grain query count : {items.Count}");
                    return items;
                }
            }

            
            

            //_logger.Info($"Grains sent to filter : {grains.Count}, after filter : {items.Count}");
            return grainQuery.ToList();
        }

        public async Task OnNextAsync(DiffModel item, StreamSequenceToken token = null)
        {
            Debug.WriteLine($"OnNextAsync called with {item.NewGrains.Count} items");
            if (item != null)
            {

                //apply filter here, that dewald has deleted once again
                if (item.NewGrains != null && item.NewGrains.Any())
                {
                    item.NewGrains = await ApplyFilter(item.NewGrains);
                }

                if (item.NewGrains != null && (item.NewGrains.Any() || item.RemovedGrains.Any()))
                {
                    Debug.WriteLine($"Sending {item.NewGrains.Count} new grains to the observers..");
                    _subsManager.Notify(s => s.GrainsUpdated(item));
                }
            }
        }

        public Task OnCompletedAsync()
        {
            throw new NotImplementedException();
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
