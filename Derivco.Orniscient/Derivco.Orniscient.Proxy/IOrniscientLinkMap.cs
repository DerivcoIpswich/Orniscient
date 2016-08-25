using System;
using Derivco.Orniscient.Proxy.Attributes;

namespace Derivco.Orniscient.Proxy
{
    public interface IOrniscientLinkMap
    {
        OrniscientGrain GetLinkFromType(string type);
        OrniscientGrain GetLinkFromType(Type type);
    }
}