using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace Derivco.Orniscient.TestImplementation
{
    public interface IFooGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }

    [Proxy.Attributes.Orniscient(linkFromType: typeof(SubGrain), linkType: LinkType.SameId,colour:"lightblue" )]
    public class FooGrain : Grain,IFooGrain {
        public Task KeepAlive()
        {
            return TaskDone.Done;
        }
    }
}
