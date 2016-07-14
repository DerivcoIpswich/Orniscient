using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class TypeMethodGrain : Grain, ITypeMethodsGrain
    {
        private List<GrainMethod> _methods = new List<GrainMethod>();

        public Task<List<GrainMethod>> GetAvailableMethods()
        {
            if (_methods != null && !_methods.Any())
            {
                var typeName = this.GetPrimaryKeyString();

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
                        .Select(m => new GrainMethod
                        {
                            Name = m.Name,
                            Parameters =
                                m.GetParameters()
                                    .Select(p => new GrainMethodParameters {Name = p.Name, Type = p.ParameterType.ToString()})
                                    .ToList()
                        }).ToList();
                }
            }
            return Task.FromResult(_methods);
        }
    }
}