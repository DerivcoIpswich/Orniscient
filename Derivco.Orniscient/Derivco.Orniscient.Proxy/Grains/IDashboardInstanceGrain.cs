using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;

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
}
