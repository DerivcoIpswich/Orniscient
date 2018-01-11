using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class DashboardInstanceGrain : Grain, IDashboardInstanceGrain
    {
        private IOrniscientLinkMap _orniscientLink;

        public DashboardInstanceGrain()
        {}

        internal DashboardInstanceGrain(IGrainIdentity identity, IGrainRuntime runtime,IOrniscientLinkMap orniscientLink) : base(identity, runtime)
        {
            _orniscientLink = orniscientLink;
        }

        private IDashboardCollectorGrain _dashboardCollectorGrain;
        private AppliedFilter _currentFilter;
        private Logger _logger;
        private IStreamProvider _streamProvider;

        private int SessionId => (int) this.GetPrimaryKeyLong();
        
        private int _summaryViewLimit = 100; 
        internal List<UpdateModel> CurrentStats = new List<UpdateModel>();
        private bool InSummaryMode => CurrentStats != null && CurrentStats.Count > _summaryViewLimit;
        private IAsyncStream<DiffModel> _dashboardInstanceStream;

        public override async Task OnActivateAsync()
        {
            if (_orniscientLink==null)
            {
                _orniscientLink = OrniscientLinkMap.Instance;
            }

            await base.OnActivateAsync();
            _logger = GetLogger("DashboardInstanceGrain");

            _dashboardCollectorGrain = GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

            _streamProvider = GetStreamProvider(StreamKeys.StreamProvider);
            var stream = _streamProvider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientChanges);
            await stream.SubscribeAsync(OnNextAsync);

            _dashboardInstanceStream = _streamProvider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientClient);
            _logger.Info("DashboardInstanceGrain Activated.");
        }

        public async Task<DiffModel> GetAll(AppliedFilter filter = null)
        {
            _currentFilter = filter;
            CurrentStats = await ApplyFilter(await _dashboardCollectorGrain.GetAll());

            _logger.Verbose($"GetAll called DashboardInstance Grain [Id : {this.GetPrimaryKeyLong()}][CurrentStatsCount : {CurrentStats?.Count}]");

            //if we are over the summaryViewLimit we need to keep the summary model details, then the counts will be updated every time new items are pushed here from the DashboardCollecterGrain/
            if (InSummaryMode)
            {
                return new DiffModel
                {
                    SummaryView = InSummaryMode,
                    NewGrains = GetGrainSummaries(),
                    SummaryViewLinks = GetGrainSummaryLinks()
                };
            }

            //under normal circumstances we just returned the detail grains.
            return new DiffModel
            {
                NewGrains = CurrentStats
            };
        }

        public Task<GrainType[]> GetGrainTypes()
        {
            return _dashboardCollectorGrain.GetGrainTypes();
        }

        public Task SetSummaryViewLimit(int limit)
        {
            _summaryViewLimit = limit > 0 ? limit : _summaryViewLimit;
            return Task.CompletedTask;
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
                    p => (string.IsNullOrEmpty(_currentFilter.GrainId) || p.GrainId.ToLower().Contains(_currentFilter.GrainId.ToLower())) &&
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
                    var grainIdsGrainType = new List<string>();

                    if (appliedTypeFilter?.SelectedValues != null && appliedTypeFilter.SelectedValues.Any())
                    {
                        //fetch the filters
                        var filterGrain = GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
                        var currentTypeFilters = await filterGrain.GetFilters(_currentFilter.TypeFilters.Select(p => p.TypeName).ToArray());

                        foreach (var currentTypeFilter in currentTypeFilters)
                        {
                            grainIdsGrainType.AddRange(currentTypeFilter.Filters.
                                Where(
                                    p =>
                                        appliedTypeFilter.SelectedValues.ContainsKey(p.FilterName) &&
                                        appliedTypeFilter.SelectedValues[p.FilterName].Contains(p.Value)
                                ).Select(p => p.GrainId).ToList());
                        }
                    }
                    filterList.Add(sourceGrainType, grainIdsGrainType);
                }
                grainQuery = grainQuery.Where(p => filterList.ContainsKey(p.Type) && (filterList[p.Type] == null || filterList[p.Type].Any() == false || filterList[p.Type].Contains(p.GrainId)));
            }

            return grainQuery.ToList();
        }

        public async Task OnNextAsync(DiffModel item, StreamSequenceToken token = null)
        {
            _logger.Verbose($"OnNextAsync called with {item.NewGrains.Count} new grains, {item.RemovedGrains.Count} removed grains");

            List<UpdateModel> previousSummary = null;
            if (InSummaryMode)
            {
                previousSummary = GetGrainSummaries();
            }

            item.NewGrains = await ApplyFilter(item.NewGrains);
            CurrentStats?.AddRange(item.NewGrains);

            if (item.RemovedGrains?.Any() == true)
            {
                CurrentStats = CurrentStats?.Where(p => item.RemovedGrains.All(q => q != p.Id)).ToList();
            }


            if (InSummaryMode)
            {
                var updatedSummary = GetGrainSummaries();
                var diffModel = new DiffModel
                {
                    SummaryView = InSummaryMode,
                    TypeCounts = item.TypeCounts,
                    NewGrains = updatedSummary,
                    SummaryViewLinks = GetGrainSummaryLinks(),
                    SessionId = SessionId
                };

                if (previousSummary != null && previousSummary.Count > 0)
                {
                    var removed = previousSummary.Where(grain => !updatedSummary.Any(currentGrain => currentGrain.Id == grain.Id)).Select(grain => grain.Id).ToList();
                    diffModel.RemovedGrains = removed;
                }
                _logger.Verbose($"DashboardInstanceGrain sending out {diffModel.NewGrains.Count} new grains, {diffModel.RemovedGrains.Count} removed grains");
                await _dashboardInstanceStream.OnNextAsync(diffModel);
            }
            else
            {
                if (previousSummary != null)
                {
                    if (item.RemovedGrains == null)
                    {
                        item.RemovedGrains = new List<string>();
                    }
                    item.RemovedGrains.AddRange(previousSummary.Select(grain => grain.Id));
                    item.NewGrains = new List<UpdateModel>(CurrentStats);
                }

                if (item.NewGrains?.Any() == true || item.RemovedGrains?.Any() == true)
                {
                    item.SummaryView = InSummaryMode;
                    item.SessionId = SessionId;
                    _logger.Verbose($"DashboardInstanceGrain sending out {item.NewGrains?.Count} new grains, {item.RemovedGrains?.Count} removed grains");
                    await _dashboardInstanceStream.OnNextAsync(item);
                }
            }
        }

        private List<Link> GetGrainSummaryLinks()
        {
            var summaryLinks = new List<Link>();

            foreach (var updateModel in CurrentStats)
            {
                var orniscientInfo = _orniscientLink.GetLinkFromType(updateModel.Type);
                if (orniscientInfo.HasLinkFromType)
                {
                    var linkToGrain = CurrentStats.FirstOrDefault(p => p.Id == updateModel.LinkToId);
                    if (linkToGrain != null)
                    {
                        var linkToGrainSummaryId = $"{linkToGrain.Type}_{linkToGrain.Silo}";
                        var fromGrainSummaryId = $"{updateModel.Type}_{updateModel.Silo}";
                        var link = summaryLinks.FirstOrDefault(p => p.FromId == fromGrainSummaryId && p.ToId == linkToGrainSummaryId);
                        if (link != null)
                        {
                            link.Count++;
                        }
                        else
                        {
                            summaryLinks.Add(new Link
                            {
                                Count = 1,
                                FromId = fromGrainSummaryId,
                                ToId = linkToGrainSummaryId
                            });
                        }
                    }
                }
            }
            return summaryLinks;
        }

        private List<UpdateModel> GetGrainSummaries()
        {
            return CurrentStats.GroupBy(grain => new {grain.Type, grain.Silo, grain.Colour})
                .Select(grp => new UpdateModel
                {
                    Type = grp.Key.Type,
                    Silo = grp.Key.Silo,
                    Colour = grp.Key.Colour,
                    Count = grp.Count(),
                    GrainId = $"{grp.Key.Type}_{grp.Key.Silo}",
                    Id = $"{grp.Key.Type}_{grp.Key.Silo}"
                })
                .ToList();
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}