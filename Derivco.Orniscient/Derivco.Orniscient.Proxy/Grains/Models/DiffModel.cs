using System;
using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class DiffModel
    {
        public List<UpdateModel> NewGrains { get; set; }
        public List<Guid> RemovedGrains { get; set; }
    }
}