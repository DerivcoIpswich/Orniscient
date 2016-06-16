using System.Collections.Generic;

namespace Derivco.Orniscient.Proxy.Filters
{
    public class AppliedFilter
    {
        public string[] SelectedSilos { get; set; }
        public string GrainId { get; set; }
        public List<AppliedTypeFilter> TypeFilters { get; set; }
    }

    public class AppliedTypeFilter
    {
        public string TypeName { get; set; }
        public Dictionary<string,IEnumerable<string>> SelectedValues { get; set; }

        public IEnumerable<string> GetSelectedValuesForFilterName(string filterName)
        {
            return SelectedValues?[filterName];
        }
    }
}
