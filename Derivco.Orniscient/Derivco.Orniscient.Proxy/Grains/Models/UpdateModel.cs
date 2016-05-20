using System;
using System.Linq;
using Derivco.Orniscient.Proxy.Extensions;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class UpdateModel
    {
        public string ActivationId { get; set; }
        public string Silo { get; set; }
        public string Type { get; set; }
        public string TypeShortName => Type.Split('.').Last();
        public string GrainName => $"{TypeShortName} ({Guid.ToInt()})";
        public string Id => $"{TypeShortName}_{Guid}";
        public Guid Guid { get; set; }
        public string LinkToId { get; set; }
        public string Colour { get; set; }
    }
}