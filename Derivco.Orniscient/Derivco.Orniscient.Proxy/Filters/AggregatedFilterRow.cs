using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Filters
{
    public class AggregatedFilterRow
    {
        public string Type { get; set; }
        public string FilterName { get; set; }
        public List<string> Values { get; set; }
    }
}
