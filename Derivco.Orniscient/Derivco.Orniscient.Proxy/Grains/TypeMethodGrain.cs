using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Derivco.Orniscient.Proxy.Grains.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.Grains
{
    public class TypeMethodGrain : Grain, ITypeMethodsGrain
    {
        private List<GrainMethod> _methods = new List<GrainMethod>();
        public override async Task OnActivateAsync()
        {
            await HydrateMethodList();
            await base.OnActivateAsync();
        }

        public Task<List<GrainMethod>> GetAvailableMethods()
        {
            return Task.FromResult(_methods);
        }

        private IGrain GetGrainReference(string id, GrainMethod method)
        {
            if (method != null)
            {
                var grainInterface = GetGrainInterfaceType(GetTypeFromString(this.GetPrimaryKeyString()));
                var grainKeyType = GetGrainKeyType(grainInterface);
                var grainKey = GetGrainKeyFromType(grainKeyType, id);

                var grainReference = typeof(GrainFactory)
                                            .GetMethod("GetGrain", new[] { grainKeyType, typeof(string) })
                                            .MakeGenericMethod(grainInterface)
                                            .Invoke(GrainFactory, new[] { grainKey, null }) as IGrain;

                if (method.InterfaceForMethod == grainInterface.FullName)
                {
                    return grainReference;
                }

                var interfaceForMethod = GetTypeFromString(method.InterfaceForMethod);

                return typeof(GrainExtensions)
                    .GetMethod("AsReference")
                    .MakeGenericMethod(interfaceForMethod)
                    .Invoke(grainReference, new object[] { grainReference }) as IGrain;
            }
            return null;
        }

        public async Task<object> InvokeGrainMethod(string id, string methodId, string parametersJson)
        {
            var method = _methods.FirstOrDefault(p => p.MethodId == methodId);
            if (method != null)
            {
                var grain = GetGrainReference(id, method);
                if (grain != null)
                {
                    //invoke the method in the grain.
                    var parameters = BuildParameterObjects(JArray.Parse(parametersJson));

                    dynamic methodInvocation = grain.GetType()
                        .GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public, null,
                            method.Parameters.Select(p => GetTypeFromString(p.Type)).ToArray(), null)
                        .Invoke(grain, parameters);

                    return await methodInvocation;
                }
            }
            return null;
        }

        private Task HydrateMethodList()
        {
            var grainType = GetTypeFromString(this.GetPrimaryKeyString());
            _methods = new List<GrainMethod>();
            
            foreach (var @interface in grainType.GetInterfaces())
            {
                if (@interface.GetInterfaces().Contains(typeof(IAddressable)))
                {
                    _methods.AddRange(@interface.GetMethods()
                        .Where(
                            p =>
                                grainType.GetMethods()
                                    .Where(q => Attribute.IsDefined(q, typeof (OrniscientMethod)))
                                    .Any(r => r.Name == p.Name))
                        .Select(m => new GrainMethod
                        {
                            Name = m.Name,
                            InterfaceForMethod = @interface.FullName,
                            MethodId = Guid.NewGuid().ToString(),
                            Parameters = m.GetParameters()
                                .Select(p => new GrainMethodParameters
                                {
                                    Name = p.Name,
                                    Type = p.ParameterType.ToString(),
                                    IsComplexType = !p.ParameterType.IsValueType && p.ParameterType != typeof (string)
                                }).ToList()
                        }));
                }
            }
            return TaskDone.Done;
        }

        private static object[] BuildParameterObjects(JArray parametersArray)
        {
            var parameterObjects = new List<object>();
            foreach (var parameter in parametersArray)
            {
                var type = GetTypeFromString(parameter["type"].ToString());
                if (!string.IsNullOrEmpty(parameter["value"].ToString()))
                {
                    var value = JsonConvert.DeserializeObject(parameter["value"].ToString(), type);
                    parameterObjects.Add(value);
                }
                else
                {
                    parameterObjects.Add(null);
                }
            }
            return parameterObjects.ToArray();
        }

        private static object GetGrainKeyFromType(Type grainKeyType, string id)
        {
            if (grainKeyType == typeof(Guid))
            {
                return Guid.Parse(id);
            }
            if (grainKeyType == typeof(int))
            {
                return int.Parse(id);
            }
            if (grainKeyType == typeof(string))
            {
                return id;
            }
            return null;
        }

        private static Type GetTypeFromString(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(typeName);
                    if (type != null)
                        break;
                }
            }
            return type;
        }

        private static Type GetGrainInterfaceType(Type grainType)
        {
            return grainType?.GetInterfaces()
                .FirstOrDefault(i => grainType.GetInterfaceMap(i).TargetMethods.Any(m => m.DeclaringType == grainType) &&
                                     i.Name.Contains(grainType.Name));
        }

        private static Type GetGrainKeyType(Type grainInterface)
        {
            var grainKeyInterface = grainInterface.GetInterfaces().FirstOrDefault(i => i.Name.Contains("Key"));
            if (grainKeyInterface != null)
            {
                if (grainKeyInterface.IsAssignableFrom(typeof(IGrainWithGuidKey)))
                {
                    return typeof(Guid);
                }
                if (grainKeyInterface.IsAssignableFrom(typeof(IGrainWithIntegerKey)))
                {
                    return typeof(int);
                }
                if (grainKeyInterface.IsAssignableFrom(typeof(IGrainWithStringKey)))
                {
                    return typeof(string);
                }
            }
            return null;
        }
    }
}