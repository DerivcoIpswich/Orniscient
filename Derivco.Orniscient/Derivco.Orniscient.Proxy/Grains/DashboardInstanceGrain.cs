using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class DashboardInstanceGrain : Grain, IDashboardInstanceGrain, IAsyncObserver<DiffModel>
    {
        private int CurrentGrianCount { get; set; }
        private ObserverSubscriptionManager<IOrniscientObserver> _subsManager;
        private IDashboardCollectorGrain _dashboardCollectorGrain;
        private AppliedFilter _currentFilter;
        private Logger _logger;


        private List<UpdateModel> _summaries = new List<UpdateModel>();


        private int _summaryViewLimit = 
            100; //TODO : Get this from config when this grain is started.....

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            _logger = GetLogger("DashboardInstanceGrain");

            _dashboardCollectorGrain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
            _subsManager = new ObserverSubscriptionManager<IOrniscientObserver>();

            var streamProvider = GetStreamProvider(StreamKeys.StreamProvider);
            var stream = streamProvider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientChanges);
            await stream.SubscribeAsync<DiffModel>(OnNextAsync);

            _logger.Info("DashboardInstanceGrain Activated.");
        }

        public async Task<DiffModel> GetAll(AppliedFilter filter = null)
        {
            _currentFilter = filter;
            var allGrains = await ApplyFilter(await _dashboardCollectorGrain.GetAll());

            //if we are over the summaryViewLimit we need to keep the summary model details, then the counts will be updated every time new items are pushed here from the DashboardCollecterGrain/
            if (allGrains.Count > _summaryViewLimit)
            {
                _summaries = GetGrainSummaries(allGrains);
                return new DiffModel()
                {
                    SummaryView = allGrains.Count > _summaryViewLimit,
                    NewGrains = GetGrainSummaries(allGrains)
                };
            }

            //under normal circumstances we just returned the detail grains.
            return new DiffModel()
            {
                SummaryView = allGrains.Count > _summaryViewLimit,
                NewGrains = allGrains
            };
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

        public Task<GrainType[]> GetGrainTypes()
        {
            return _dashboardCollectorGrain.GetGrainTypes();
        }

        private async Task<List<UpdateModel>> ApplyFilter(List<UpdateModel> grains = null)
        {
            _logger.Verbose($"Applying filters");
            if (_currentFilter == null || grains == null)
                return grains;

            //order of filtering applies here.
            //1. Grain Id & Silo
            var grainQuery =
                grains.Where(
                    p => (string.IsNullOrEmpty(_currentFilter.GrainId) || p.GrainId.Contains(_currentFilter.GrainId)) &&
                         (_currentFilter.SelectedSilos == null || _currentFilter.SelectedSilos.Length == 0 ||
                          _currentFilter.SelectedSilos.Contains(p.Silo)));

            //2. Type filters
            if (_currentFilter.TypeFilters != null)
            {
                var filterList = new Dictionary<string, List<string>>();
                var sourceGrainTypes =
                    grains.Where(p => _currentFilter.TypeFilters.Any(cf => cf.TypeName == p.Type))
                        .Select(p => p.Type)
                        .Distinct()
                        .ToList();
                foreach (var sourceGrainType in sourceGrainTypes)
                {
                    var appliedTypeFilter = _currentFilter.TypeFilters.FirstOrDefault(p => p.TypeName == sourceGrainType);
                    List<string> grainIdsGrainType = null;

                    if (appliedTypeFilter?.SelectedValues != null && appliedTypeFilter.SelectedValues.Any())
                    {
                        //fetch the filters
                        var filterGrain = GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
                        var currentTypeFilters = await filterGrain.GetFilters(_currentFilter.TypeFilters.Select(p => p.TypeName).ToArray());

                        foreach (var currentTypeFilter in currentTypeFilters)
                        {
                            grainIdsGrainType = currentTypeFilter.Filters.
                                Where(
                                    p =>
                                        appliedTypeFilter.SelectedValues.ContainsKey(p.FilterName) &&
                                        appliedTypeFilter.SelectedValues[p.FilterName].Contains(p.Value)
                                ).Select(p => p.GrainId).ToList();
                        }
                    }
                    filterList.Add(sourceGrainType, grainIdsGrainType);
                }
                grainQuery = grainQuery.Where(p => filterList.ContainsKey(p.Type) && (filterList[p.Type] == null || filterList[p.Type].Contains(p.GrainId)));
            }

            return grainQuery.ToList();
        }

        public async Task OnNextAsync(DiffModel item, StreamSequenceToken token = null)
        {
            _logger.Verbose($"OnNextAsync called with {item.NewGrains.Count} items");
            item.NewGrains = await ApplyFilter(item.NewGrains);

            var changedSummaries = GetGrainSummaries(item.NewGrains);
            foreach (var updateModel in changedSummaries)
            {
                var summary = _summaries.FirstOrDefault(p => p.Type == updateModel.Type && p.Silo == updateModel.Silo);
                if (summary != null)
                {
                    summary.Count += updateModel.Count;
                }
                else
                {
                    _summaries.Add(updateModel);
                }
            }

            if (_summaries != null && _summaries.Sum(p => p.Count) > _summaryViewLimit)
            {
                _subsManager.Notify(s=>s.GrainsUpdated(new DiffModel()
                { 
                    SummaryView = true,
                    TypeCounts = item.TypeCounts,
                    NewGrains = _summaries
                }));
            }
            else
            {
                //we are in detailed mode
                _logger.Verbose($"OnNextAsync called with {item.NewGrains.Count} items");
                if (item.NewGrains != null && item.NewGrains.Any())
                {
                    item.NewGrains = await ApplyFilter(item.NewGrains);
                }

                if (item.NewGrains != null && (item.NewGrains.Any() || item.RemovedGrains.Any()))
                {
                    _logger.Verbose($"Sending {item.NewGrains.Count} new grains to the observers");
                    _subsManager.Notify(s => s.GrainsUpdated(item));
                }
            }
        }

        private static List<UpdateModel> GetGrainSummaries(IEnumerable<UpdateModel> grains)
        {
            var changedSummaries = (from grain in grains
                                    group grain by new { grain.Type, grain.Silo, grain.Colour }
                                    into grp
                                    select new UpdateModel()
                                    {
                                        Type = grp.Key.Type,
                                        Silo = grp.Key.Silo,
                                        Colour = grp.Key.Colour,
                                        Count = grp.Count(),
                                        GrainId = $"{grp.Key.Type}_{grp.Key.Silo}",
                                        Id = $"{grp.Key.Type}_{grp.Key.Silo}"
                                    }).ToList();
            return changedSummaries;
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }
    }
}