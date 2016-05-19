using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Derivco.Orniscient.TestImplementation
{
    public interface IFirstGrain : IGrainWithGuidKey
    {
        Task KeepAlive();
    }
}
