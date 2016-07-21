using System;
using System.Collections.Generic;
using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class GrainMethod
    {
        public string Name { get; set; }
        public List<GrainMethodParameters> Parameters { get; set; }
        public string InterfaceForMethod { get; set; }
        public string MethodId { get; set; }
    }
}