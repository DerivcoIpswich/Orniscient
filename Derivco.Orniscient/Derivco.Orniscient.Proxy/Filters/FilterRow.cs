using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Filters
{
    public class FilterRow
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class FilterRowSummary : FilterRow
    {
        public FilterRowSummary(FilterRow filterRow, string grainId)
        {
            Name = filterRow.Name;
            Value = filterRow.Value;
            GrainsWithValue = new List<string>() { grainId };
        }

        public List<string> GrainsWithValue { get; set; }
    }
}
