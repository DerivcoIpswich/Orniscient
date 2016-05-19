using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Extensions;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy
{
    public interface IOrniscientGrain : IGrainWithGuidKey 
    {
        Task<List<UpdateModel>> GetAll();
        Task<DiffModel> GetChanges();
        Task Subscribe(IOrniscientObserver observer);
        Task UnSubscribe(IOrniscientObserver observer);
    }

    public class OrniscientGrain : Grain, IOrniscientGrain
    {
        private List<UpdateModel> CurrentStats { get; set; }
        private IManagementGrain _managementGrain;
        private ObserverSubscriptionManager<IOrniscientObserver> _subsManager;

        public override async Task OnActivateAsync()
        {
            _managementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
            //Timer to send the changes down to the dashboard every x minutes....
            await _Hydrate();
            RegisterTimer(p => GetChanges(), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20));
            _subsManager = new ObserverSubscriptionManager<IOrniscientObserver>();
            await base.OnActivateAsync();
        }

        private async Task _Hydrate()
        {
            CurrentStats = await _GetAllFromCluster();
        }

        private static UpdateModel _FromGrainStat(DetailedGrainStatistic grainStatistic)
        {
            return new UpdateModel()
            {
                Guid = grainStatistic.GrainIdentity.PrimaryKey,
                Type = grainStatistic.GrainType,
                Silo = grainStatistic.SiloAddress.ToString()
            };
        }

        private async Task<List<UpdateModel>> _GetAllFromCluster()
        {
            var types = await _managementGrain.GetActiveGrainTypes();
            var detailedStats = await _managementGrain.GetDetailedGrainStatistics(types.Where(p => p.Contains("Derivco")).ToArray()); ;
            if (detailedStats != null && detailedStats.Any())
            {
                return detailedStats.Select(_FromGrainStat).ToList();
            }
            return null;
        }

        public Task<List<UpdateModel>> GetAll()
        {
            return Task.FromResult(CurrentStats);
        }

        public async Task<DiffModel> GetChanges()
        {
            var newStats = await _GetAllFromCluster();
            var diffModel = new DiffModel()
            {
                RemovedGrains = CurrentStats.Where(p=> newStats.All(n => n.Guid != p.Guid)).Select(p=>p.Guid).ToList(),
                NewGrains = newStats.Where(p=> CurrentStats.All(c => p.Guid != c.Guid)).ToList()
            };

            //push the diffmodel to the observer..
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
    }

    public class UpdateModel
    {
        public string Silo { get; set; }
        public string Type { get; set; }

        public string GrainName => $"{Type.Split('.').Last()} ({Guid.ToInt()})";
        public Guid Guid { get; set; }
    }

    public class DiffModel
    {
        public List<UpdateModel> NewGrains { get; set; }
        public List<Guid> RemovedGrains { get; set; }
    }
}
