using System.Collections.Generic;
using System.Linq;

namespace Derivco.Orniscient.Proxy.Filters
{
    public class TypeFilter
    {
        public TypeFilter()
        {
            Filters = new List<FilterRow>();
        }

        public string TypeName { get; set; }
        public string TypeNameShort => TypeName.Split('.').Last();
        public List<FilterRow> Filters { get; set; }
    }
}
