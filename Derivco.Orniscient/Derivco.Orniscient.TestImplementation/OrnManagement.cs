using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.TestImplementation
{
    public class OrnManagement
    {
        public async Task<DetailedGrainStatistic[]> GetGrainStats()
        {
            var mngGrain = GrainClient.GrainFactory.GetGrain<IManagementGrain>(0);

            var types = await mngGrain.GetActiveGrainTypes();

            var grnStats= await mngGrain.GetDetailedGrainStatistics(types.Where(p=> p.Contains("Spinsport")).ToArray());
            return grnStats;
        }
    }
}
