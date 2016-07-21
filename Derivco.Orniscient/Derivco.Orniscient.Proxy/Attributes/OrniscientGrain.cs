using System;

namespace Derivco.Orniscient.Proxy.Attributes
{
    public class OrniscientGrain : Attribute
    {
        public OrniscientGrain(Type linkFromType=null,LinkType linkType=LinkType.SameId,string colour="", Type filterGrain=null,string defaultLinkFromTypeId="")
        {

            LinkFromType = linkFromType;
            LinkType = linkType;
            Colour = colour;
            FilterGrain = filterGrain;
            DefaultLinkFromTypeId = defaultLinkFromTypeId;
        }

        public Type LinkFromType { get; }
        public LinkType LinkType { get; private set; }
        public string Colour { get; set; }
        public Type FilterGrain { get; set; }
        public string DefaultLinkFromTypeId { get; set; }
        public bool HasLinkFromType => LinkFromType != null;
        public IdentityTypes IdentityType { get; internal set; }
    }

    public enum LinkType
    {
        SingleInstance,
        SameId
    }

    public enum IdentityTypes
    {
        Int,
        Guid,
        String,
        NotFound
    }
}
