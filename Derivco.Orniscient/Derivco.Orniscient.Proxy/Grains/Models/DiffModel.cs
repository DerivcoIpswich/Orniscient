using System;
using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class DiffModel
    {
        public List<UpdateModel> NewGrains { get; set; }
        public List<Guid> RemovedGrains { get; set; }

        public List<TypeCounter> TypeCounts { get; set; }
    }

    public class TypeCounter
    {
        public string TypeName { get; set; }
        public int Total { get; set; }
    }
}