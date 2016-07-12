using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IDashboardInstanceGrain : IGrainWithIntegerKey
    {
        Task<DiffModel> GetAll(AppliedFilter filter = null);
        Task<GrainType[]> GetGrainTypes();
        Task SetSummaryViewLimit(int limit);
    }
}