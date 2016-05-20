using System;

namespace Derivco.Orniscient.Proxy.Attributes
{
    public class Orniscient : Attribute
    {
        public Orniscient(Type linkFromType=null,LinkType linkType=LinkType.SameId,string colour="")
        {
            LinkFromType = linkFromType;
            LinkType = linkType;
            Colour = colour;
        }

        public Type LinkFromType { get; }
        public LinkType LinkType { get; private set; }
        public string Colour { get; set; }
        public bool HasLinkFromType => LinkFromType != null;


    }

    public enum LinkType
    {
        SingleInstance,
        SameId
    }
}
