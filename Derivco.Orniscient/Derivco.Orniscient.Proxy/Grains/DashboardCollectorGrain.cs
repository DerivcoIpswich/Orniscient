using System;
using System.Collections.Generic;
using System.Configuration;
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
        private GrainType[] _filteredTypes;
        private Logger _logger;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

			_managementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
			_logger = GetLogger();
            CurrentStats = new List<UpdateModel>();

            await Hydrate();

            var configTimerPeriods = ConfigurationManager.AppSettings["DashboardCollectorGrainTimerPeriods"];
            var timerPeriods = configTimerPeriods?.Split(',').Select(int.Parse).ToArray() ?? new[] {2, 6};

            RegisterTimer(p => GetChanges(), null, TimeSpan.FromSeconds(timerPeriods[0]), TimeSpan.FromSeconds(timerPeriods[1]));
            await GrainFactory.GetGrain<IFilterGrain>(Guid.Empty).KeepAlive();
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

	    public async Task SetTypeFilter(GrainType[] types)
	    {
		    _filteredTypes = types;
		    await Hydrate();
	    }

	    public async Task<string[]> GetSilos()
	    {
		    var silos = await _managementGrain.GetHosts(true);
		    return silos.Keys.Select(p => p.ToString()).ToArray();
	    }

	    public async Task<GrainType[]> GetGrainTypes()
	    {
		    var types = await _managementGrain.GetActiveGrainTypes();
		    return types?.Where(p => _filteredTypes == null || _filteredTypes.Any(ft => ft.FullName == p))
			    .Select(p => new GrainType(p))
			    .OrderBy(t => t.ShortName)
			    .ToArray();
	    }

	    public Task<List<string>> GetGrainIdsForType(string type)
	    {
		    var ids = CurrentStats.Where(s => s.Type == type).Select(g => g.GrainId).ToList();
		    return Task.FromResult(ids);
	    }

	    private async Task Hydrate()
	    {
		    CurrentStats = await _GetAllFromCluster();
	    }

	    private async Task<DiffModel> GetChanges()
	    {
		    var newStats = await _GetAllFromCluster() ?? new List<UpdateModel>();

		    var diffModel = new DiffModel
		    {
			    RemovedGrains = CurrentStats?.Where(p => newStats.All(n => n.GrainId != p.GrainId)).Select(p => p.GrainId).ToList(),
			    NewGrains = newStats.Where(n => CurrentStats?.Any(c => c.Id == n.Id) == false).ToList(),
			    TypeCounts = newStats.GroupBy(p => p.TypeShortName).Select(p => new TypeCounter { TypeName = p.Key, Total = p.Count() }).ToList()
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

	    private static UpdateModel _FromGrainStat(DetailedGrainStatistic grainStatistic)
        {
            var model = new UpdateModel
            {
                Type = grainStatistic.GrainType,
                Silo = grainStatistic.SiloAddress.ToString()
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
                return detailedStats.Select(_FromGrainStat).OrderBy(s => s.TypeShortName).ToList();
            }
            _logger.Verbose("_GetAllFromCluster called [nothing returned from ManagementGrain]");
            return null;
        }
    }
}