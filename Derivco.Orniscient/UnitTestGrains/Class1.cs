using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.TestImplementation.Attributes;
using Orleans;
using Orleans.Runtime;

namespace OrleansManager
{
    public class Class1
    {
        private static IManagementGrain systemManagement;
        
        public async Task Test()
        {

            systemManagement = GrainClient.GrainFactory.GetGrain<IManagementGrain>(1);
            var silos = GetSiloAddresses();
            if (silos == null || silos.Count == 0) return;

            var siloControl = GrainClient.InternalGrainFactory.GetSystemTarget<ISiloControl>(Constants.SiloControlId, silos[0]);
            var grainStats = await siloControl.GetGrainStatistics();
            if (grainStats != null)
            {
                foreach (var grainStat in grainStats)
                {
                    //We have the silo name, since we will be going through each silo here..no need to determine this from the string, 
                    
                    //get the grain name, id and silo name from the grainstat. Then we might need to get more information from an attribute or something.
                    var identitysdtring = grainStat.Item1.IdentityString;
                    var grainTypeName = grainStat.Item2;

                    Assembly asm = typeof(Orniscient).Assembly;
                    var grainType = asm.GetType(grainTypeName);
                    if (grainType != null)
                    {
                        var ornAttribute =
                            grainType.GetCustomAttributes(typeof (Orniscient), true).FirstOrDefault() as Orniscient;
                        if (ornAttribute != null)
                        {
                            string temp = $"SweeeEEETTTTTTT we got the name {ornAttribute.DisplayName}";
                        }
                    }

                    //IdentityString = "*grn/83371988/434f3f72f04d7d01668bde3963472fbb03ffffff83371988-0xE90FF58C"
                    //                  "*grn/typestring/idstring
                    //


                }
            }
            
            

        }

        private static List<SiloAddress> GetSiloAddresses()
        {
            IList<Uri> gateways = GrainClient.Gateways;
            if (gateways.Count >= 1)
                return gateways.Select(Utils.ToSiloAddress).ToList();

            return null;
        }
    }
}
