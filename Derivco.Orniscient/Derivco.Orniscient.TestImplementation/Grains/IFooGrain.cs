using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace Derivco.Orniscient.TestImplementation.Grains
{
    public interface IFooGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }

    [OrniscientGrain(typeof(SubGrain), LinkType.SameId,"lightblue" )]
    public class FooGrain : Grain,IFooGrain {
        public Task KeepAlive()
        {
            return TaskDone.Done;
        }
    }
}
