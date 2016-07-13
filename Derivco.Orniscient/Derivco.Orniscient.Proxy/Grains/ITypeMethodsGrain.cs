using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface ITypeMethodsGrain : IGrainWithStringKey
    {
        Task<List<GrainMethod>> GetAvailableMethods();

        //TODO : this needs to be defined
        //Task InvokeMethod(string name);
    }


    public class TypeMethodGrain : Grain, ITypeMethodsGrain
    {
        private List<GrainMethod> _methods = new List<GrainMethod>();

        public Task<List<GrainMethod>> GetAvailableMethods()
        {
            if (_methods != null && !_methods.Any())
            {
                string typeName = this.GetPrimaryKeyString();


                var grainType = Type.GetType(typeName);
                if (grainType == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        grainType = a.GetType(typeName);
                        if(grainType != null)
                            break;
                    }
                }

                //type could still be null here
                if (grainType != null)
                {
                    _methods = grainType.GetMethods()
                        .Where(m => Attribute.IsDefined(m, typeof (OrniscientMethod)))
                        .Select(m => new GrainMethod()
                        {
                            Name = m.Name
                        }).ToList();
                }
            }
            return Task.FromResult(_methods);
        }
    }





    public class GrainMethod
    {
        public string Name { get; set; }
        public List<GrainMethodParameters> Parameters { get; set; }
    }

    public class GrainMethodParameters
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class OrniscientMethod : Attribute
    {
        
    }

    
}
