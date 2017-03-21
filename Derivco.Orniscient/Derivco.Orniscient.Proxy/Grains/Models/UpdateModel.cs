using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class UpdateModel
    {
        public string Silo { get; set; }
        public string Type { get; set; }
        public string TypeShortName => Type.Split('.').Last();
        public string GrainName => $"{TypeShortName} ({GrainId})";
        public string Id { get; set; }
        public string GrainId { get; set; }
        public string LinkToId { get; set; }
        public string Colour { get; set; }
        public int Count { get; set; } = 1;

        public override string ToString()
        {
            return $"Type : {Type},GrainId : {GrainId}";
        }
    }
}