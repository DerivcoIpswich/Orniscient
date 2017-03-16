namespace Derivco.Orniscient.Proxy.Filters
{
    public class FilterRow
    {
	    public FilterRow()
	    {
	    }

        public FilterRow(string name,string value)
        {
            FilterName = name;
            Value = value;
        }

        public string FilterName { get; set; }
        public string Value { get; set; }
        public string GrainId { get; set; }
    }
}
