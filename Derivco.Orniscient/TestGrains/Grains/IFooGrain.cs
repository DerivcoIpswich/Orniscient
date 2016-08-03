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
        Task KeepAlive();

        Task<Dictionary<string, string>> KeepAliveOne(int? intOne, string stringOne);

        Task KeepAliveTwo(ExternalParameterType externalParameterTwo);

        Task KeepAliveThree(Dictionary<string, int> dictionaryStringIntThree);

        Task KeepAliveThree(Dictionary<int, int> dictionaryStringIntThree);
    }
}
