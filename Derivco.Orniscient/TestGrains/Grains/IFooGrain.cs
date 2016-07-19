using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Filters;
using Orleans;

namespace TestGrains.Grains
{
    public interface IFooGrain : IGrainWithGuidKey
    {
        [OrniscientMethod]
        Task KeepAlive();

        [OrniscientMethod]
        Task KeepAliveOne(int intOne, string stringOne);

        [OrniscientMethod]
        Task KeepAliveTwo(ExternalParameterType externalParameterTwo);

        [OrniscientMethod]
        Task KeepAliveThree(Dictionary<string, int> dictionaryStringIntThree);
    }
}
