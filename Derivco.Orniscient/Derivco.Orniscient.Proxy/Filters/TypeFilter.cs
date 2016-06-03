using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Filters
{
    public class TypeFilter
    {
        public TypeFilter()
        {
            Filters = new List<AggregatedFilterRow>();
        }

        public string TypeName { get; set; }
        public List<AggregatedFilterRow> Filters { get; set; }

    }
}
