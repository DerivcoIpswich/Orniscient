using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class OrniscientReportingGrain : Grain, IOrniscientReportingGrain
    {
        private List<UpdateModel> CurrentStats { get; set; }
        private IManagementGrain _managementGrain;
        private ObserverSubscriptionManager<IOrniscientObserver> _subsManager;
        private string[] filteredTypes;

        Orleans.Runtime.Logger logger;
        public override async Task OnActivateAsync()
        {
            logger = GetLogger();
            CurrentStats = new List<UpdateModel>();
            _managementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            //Timer to send the changes down to the dashboard every x minutes....
            await _Hydrate();
            RegisterTimer(p => GetChanges(), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            _subsManager = new ObserverSubscriptionManager<IOrniscientObserver>();
            await base.OnActivateAsync();
        }

        private async Task _Hydrate()
        {
            CurrentStats = await _GetAllFromCluster();
        }

        private static UpdateModel _FromGrainStat(DetailedGrainStatistic grainStatistic)
        {
            var model = new UpdateModel()
            {
                Guid = grainStatistic.GrainIdentity.PrimaryKey,
                Type = grainStatistic.GrainType,
                Silo = grainStatistic.SiloAddress.ToString()
            };

            try
            {
                model.Guid = grainStatistic.GrainIdentity.PrimaryKey;
            }
            catch (Exception)
            {
                model.Guid = Guid.NewGuid();
                Debug.WriteLine($"This guid is not cool {model.TypeShortName}");
                throw;
            }

            //need to check the linktypes
            var orniscientInfo = OrniscientLinkMap.Instance.GetLinkFromType(model.Type);
            if (orniscientInfo != null && orniscientInfo.HasLinkFromType)
            {
                var mapGuid = orniscientInfo.LinkType == LinkType.SameId ? model.Guid : Guid.Empty;
                model.LinkToId = $"{orniscientInfo.LinkFromType.ToString().Split('.').Last()}_{mapGuid}";
                model.Colour = orniscientInfo.Colour;
            }
            return model;

        }

        private async Task<List<UpdateModel>> _GetAllFromCluster()
        {
            logger.Info("_GetAllFromCluster called");
            var detailedStats = await _managementGrain.GetDetailedGrainStatistics(filteredTypes); ;
            if (detailedStats != null && detailedStats.Any())
            {
                logger.Info($"_GetAllFromCluster called [{detailedStats.Length} items returned from ManagementGrain]");
                return detailedStats.Where(p => p.Category.ToLower() == "grain").Select(_FromGrainStat).ToList();
            }

            return null;
        }

        public Task<List<UpdateModel>> GetAll()
        {
            return Task.FromResult(CurrentStats);
        }

        public Task<List<UpdateModel>> GetAll(string type)
        {
            if (CurrentStats == null) return Task.FromResult<List<UpdateModel>>(null);

            var filteredStats = CurrentStats.Where(x => x.Type == type);
            return Task.FromResult(filteredStats.ToList());
        }

        public async Task<DiffModel> GetChanges()
        {
            var newStats = await _GetAllFromCluster();

            if (CurrentStats == null)
                CurrentStats = new List<UpdateModel>();

            if (newStats == null)
                newStats = new List<UpdateModel>();


            var diffModel = new DiffModel()
            {
                RemovedGrains = CurrentStats.Where(p => newStats.All(n => n.Guid != p.Guid)).Select(p => p.Guid).ToList(),
                NewGrains = newStats.Where(p => CurrentStats.All(c => p.Guid != c.Guid)).ToList(),
                TypeCounts = newStats.GroupBy(p => p.TypeShortName).Select(p => new TypeCounter() { TypeName = p.Key, Total = p.Count()}).ToList()
            };

            //push the diffmodel to the observer..
            logger.Info($"Sending changes [{diffModel.NewGrains.Count} new grains]");
            _subsManager.Notify(s => s.GrainsUpdated(diffModel));

            //Update the CurrentStats with the latest.
            CurrentStats = newStats;
            return diffModel;
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

        public async Task SetTypeFilter(string[] types)
        {
            this.filteredTypes = types;
            await _Hydrate();
        }

        public async Task<string[]> GetSilos()
        {
            var silos = await _managementGrain.GetHosts(true);
            return silos.Keys.Select(p => p.ToString()).ToArray();
        }

        public async Task<string[]> GetGrainTypes()
        {
            var types = await _managementGrain.GetActiveGrainTypes();
            return types;
        }
    }
}