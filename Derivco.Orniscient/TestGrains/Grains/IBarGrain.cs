using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace TestGrains.Grains
{
    public interface IBarGrain : IGrainWithIntegerKey
    {
        Task KeepAlive();
    }

    [OrniscientGrain()]
    public class BarGrain : Grain, IBarGrain
    {
        public Task KeepAlive()
        {
            return TaskDone.Done;
        }
    }
}
