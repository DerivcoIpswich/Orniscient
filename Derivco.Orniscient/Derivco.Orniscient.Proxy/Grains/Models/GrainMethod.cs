using System.Collections.Generic;
using System.Reflection;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class GrainMethod
    {
        public string Name { get; set; }
        public int MethodHashCode { get; set; }
        public List<GrainMethodParameters> Parameters { get; set; }
    }
}