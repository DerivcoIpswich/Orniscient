using System;
using System.Collections.Generic;
using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class GrainMethod
    {
        public string Name { get; set; }
        public int MethodHashCode { get; set; }
        public List<GrainMethodParameters> Parameters { get; set; }
        public string InterfaceForMethod { get; set; }

        public string MethodId
        {
            get
            {
                if (Parameters == null || !Parameters.Any())
                    return Name;

                return $"{Name}_{string.Join("_", Parameters.Select(p => p.Name))}";
            }
        }
    }
}