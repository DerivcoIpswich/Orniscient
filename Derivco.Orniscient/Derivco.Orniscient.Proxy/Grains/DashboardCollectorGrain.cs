using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class DashboardCollectorGrain : Grain, IDashboardCollectorGrain
    {
        private List<UpdateModel> CurrentStats { get; set; }
        private IManagementGrain _managementGrain;
        private GrainType[] _filteredTypes = null;
        private Logger _logger;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            _logger = GetLogger();
            CurrentStats = new List<UpdateModel>();
            _managementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            //Timer to send the changes down to the dashboard every x minutes....
            await _Hydrate();
            RegisterTimer(p => GetChanges(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));
            await GrainFactory.GetGrain<IFilterGrain>(Guid.Empty).KeepAlive();
        }

        private async Task _Hydrate()
        {
            CurrentStats = await _GetAllFromCluster();
        }

        private static UpdateModel _FromGrainStat(DetailedGrainStatistic grainStatistic)
        {
            var model = new UpdateModel()
            {
                Type = grainStatistic.GrainType,
                Silo = grainStatistic.SiloAddress.ToString(),
            };

            var orniscientInfo = OrniscientLinkMap.Instance.GetLinkFromType(model.Type);
            if (orniscientInfo != null)
            {
                if (!string.IsNullOrEmpty(orniscientInfo.Colour))
                {
                    model.Colour = orniscientInfo.Colour;
                }

                switch (orniscientInfo.IdentityType)
                {

                    case IdentityTypes.String:
                        {
                            model.Id = $"{model.TypeShortName}_{grainStatistic.GrainIdentity.PrimaryKeyString}";
                            model.GrainId = grainStatistic.GrainIdentity.PrimaryKeyString;
                            break;
                        }
                    case IdentityTypes.Guid:
                        {
                            model.Id = $"{model.TypeShortName}_{grainStatistic.GrainIdentity.PrimaryKey}";
                            model.GrainId = grainStatistic.GrainIdentity.PrimaryKey.ToString();
                            break;
                        }
                    case IdentityTypes.Int:
                        {
                            //because an int key is returned as a guid, we need to turn it back to an int here.
                            model.Id = $"{model.TypeShortName}_{grainStatistic.GrainIdentity.PrimaryKeyLong}";
                            model.GrainId = grainStatistic.GrainIdentity.PrimaryKeyLong.ToString();
                            break;
                        }
                    default:
                        {
                            model.Id = grainStatistic.GrainIdentity.IdentityString;
                            model.GrainId = grainStatistic.GrainIdentity.PrimaryKeyString;
                            break;
                        }
                }

                if (orniscientInfo.HasLinkFromType)
                {
                    var mapId = orniscientInfo.LinkType == LinkType.SameId ? model.GrainId : orniscientInfo.DefaultLinkFromTypeId;
                    model.LinkToId = $"{orniscientInfo.LinkFromType.ToString().Split('.').Last()}_{mapId}";
                }
            }
            return model;
        }

        private async Task<List<UpdateModel>> _GetAllFromCluster()
        {
            _logger.Info("_GetAllFromCluster called");
            var detailedStats = await _managementGrain.GetDetailedGrainStatistics(_filteredTypes?.Select(p => p.FullName).ToArray()); ;
            if (detailedStats != null && detailedStats.Any())
            {
                _logger.Verbose($"_GetAllFromCluster called [{detailedStats.Length} items returned from ManagementGrain]");
                return detailedStats.Select(_FromGrainStat).ToList();
            }
            _logger.Verbose($"_GetAllFromCluster called [nothing returned from ManagementGrain]");
            return null;
        }

        public Task<List<UpdateModel>> GetAll()
        {
            return Task.FromResult(CurrentStats);
        }

        public Task<List<UpdateModel>> GetAll(string type)
        {
            if (CurrentStats == null)
                return Task.FromResult<List<UpdateModel>>(null);
            var filteredStats = CurrentStats.Where(x => x.Type == type);
            return Task.FromResult(filteredStats.ToList());
        }

        private async Task<DiffModel> GetChanges()
        {
            var newStats = await _GetAllFromCluster() ?? new List<UpdateModel>();

            var diffModel = new DiffModel()
            {
                RemovedGrains = CurrentStats?.Where(p => newStats.All(n => n.GrainId != p.GrainId)).Select(p => p.GrainId).ToList(),
                NewGrains = newStats.Where(n => CurrentStats?.Any(c => c.Id == n.Id) == false).ToList(),
                TypeCounts = newStats.GroupBy(p => p.TypeShortName).Select(p => new TypeCounter() { TypeName = p.Key, Total = p.Count() }).ToList()
            };

            //Update the CurrentStats with the latest.
            CurrentStats = newStats;

            _logger.Verbose($"Sending {diffModel.RemovedGrains?.Count} RemovedGrains from DashboardCollectorGrain");
            _logger.Verbose($"Sending {diffModel.NewGrains?.Count} NewGrains from DashboardCollectorGrain");
            _logger.Verbose($"Sending {diffModel.TypeCounts?.Count} TypeCounts from DashboardCollectorGrain");

            var streamProvider = GetStreamProvider(StreamKeys.StreamProvider);

            _logger.Info($"About to send the changes to the dashboardInstanceGrains");
            var stream = streamProvider.GetStream<DiffModel>(Guid.Empty, StreamKeys.OrniscientChanges);
            await stream.OnNextAsync(diffModel);
            return diffModel;
        }

        public async Task SetTypeFilter(GrainType[] types)
        {
            this._filteredTypes = types;
            await _Hydrate();
        }

        public async Task<string[]> GetSilos()
        {
            var silos = await _managementGrain.GetHosts(true);
            return silos.Keys.Select(p => p.ToString()).ToArray();
        }

        public async Task<GrainType[]> GetGrainTypes()
        {
            var types = await _managementGrain.GetActiveGrainTypes();
            return types?.Select(p => new GrainType(p)).ToArray();
        }
    }
}