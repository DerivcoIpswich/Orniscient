using Derivco.Orniscient.Proxy.Grains.Models;

namespace Derivco.Orniscient.Viewer.Models.Dashboard
{
    public class DashboardInfo
    {
        public string[] Silos { get; set; }
        public GrainType[] AvailableTypes { get; set; }
    }
}