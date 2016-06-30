using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class GrainType
    {
        public GrainType(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; set; }
        public string ShortName => !string.IsNullOrEmpty(FullName) ? FullName.Split('.').LastOrDefault() : "";
    }
}
