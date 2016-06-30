using System;

namespace Derivco.Orniscient.Proxy.Attributes
{
    public class OrniscientGrain : Attribute
    {
        public OrniscientGrain(Type linkFromType=null,LinkType linkType=LinkType.SameId,string colour="", Type filterGrain=null)
        {

            LinkFromType = linkFromType;
            LinkType = linkType;
            Colour = colour;
            FilterGrain = filterGrain;
        }

        public Type LinkFromType { get; }
        public LinkType LinkType { get; private set; }
        public string Colour { get; set; }
        public Type FilterGrain { get; set; }
        public bool HasLinkFromType => LinkFromType != null;
    }

    public enum LinkType
    {
        SingleInstance,
        SameId
    }
}
