using System.Collections.Generic;
using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class DiffModel
    {
        public DiffModel()
        {
            NewGrains = new List<UpdateModel>();
            RemovedGrains = new List<string>();
        }
        public List<UpdateModel> NewGrains { get; set; }
        public List<string> RemovedGrains { get; set; }
        public List<TypeCounter> TypeCounts { get; set; }

        public List<UpdateModel> Totals
        {
            get
            {
                if (NewGrains != null)
                {
                    var temp = from grain in NewGrains
                        group grain by new {grain.Type, grain.Silo,grain.Colour}
                        into grp
                        select new UpdateModel()
                        {
                            Type = grp.Key.Type,
                            Silo = grp.Key.Silo,
                            Colour = grp.Key.Colour,
                            Count = grp.Count(),
                            GrainId = $"{grp.Key.Type}_{grp.Key.Silo}",
                            Id = $"{grp.Key.Type}_{grp.Key.Silo}"
                            
                        };
                    return temp.ToList();

                    //TODO : Linking ?????????
                }
                return null;
            }
        }

        public bool SummaryView { get; set; }
    }
}