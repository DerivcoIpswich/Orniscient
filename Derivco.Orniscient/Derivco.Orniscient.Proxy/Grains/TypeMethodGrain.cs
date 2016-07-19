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
namespace Derivco.Orniscient.Proxy.Grains
{
    public class TypeMethodGrain : Grain, ITypeMethodsGrain
    {
        private List<GrainMethod> _methods = new List<GrainMethod>();

        public Task<List<GrainMethod>> GetAvailableMethods()
        {
            if (_methods != null && !_methods.Any())
            {
                var grainType = GetTypeFromString(this.GetPrimaryKeyString());

                //type could still be null here
                if (grainType != null)
                {
                    _methods = GetMethodList(GetGrainInterfaceType(grainType));
                }
            }
            return Task.FromResult(_methods);
        }

        public Task InvokeGrainMethod(string id, string methodName, string parametersJson)
        {
            var grainType = GetTypeFromString(this.GetPrimaryKeyString());
            var grainInterface = GetGrainInterfaceType(grainType);

            if (grainInterface != null)
            {
                var parameters = BuildParameterObjects(JArray.Parse(parametersJson));
                var grainMethod = grainInterface.GetMethod(methodName, parameters.Select(s => s.GetType()).ToArray());
                if (grainMethod != null)
                {
                    var grainKeyType = GetGrainKeyType(grainInterface);
                    var grainKey = GetGrainKeyFromType(grainKeyType, id);

                    var getGrainMethod = ReflectGetGrainMethod(grainKeyType, grainInterface);
                    var grainReference = getGrainMethod.Invoke(GrainFactory, new[] {grainKey, null});

                    //JUST DO IT!!!
                    grainMethod.Invoke(grainReference, parameters);
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
                var value = JsonConvert.DeserializeObject(parameter["value"].ToString(), type);

                if (value != null)
                {
                    parameterObjects.Add(value);
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
            if (grainKeyType == typeof (int))
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

        private static List<GrainMethod> GetMethodList(Type grainType)
        {
            return grainType.GetMethods()
                .Where(m => Attribute.IsDefined(m, typeof(OrniscientMethod)))
                .Select(m => new GrainMethod
                {
                    Name = m.Name,
                    Parameters =
                        m.GetParameters()
                            .Select(p => new GrainMethodParameters { Name = p.Name, Type = p.ParameterType.ToString() })
                            .ToList()
                }).ToList();
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
                if (grainKeyInterface.IsAssignableFrom(typeof (IGrainWithGuidKey)))
                {
                    return typeof (Guid);

                }
                if (grainKeyInterface.IsAssignableFrom(typeof (IGrainWithIntegerKey)))
                {
                    return typeof (int);

                }
                if (grainKeyInterface.IsAssignableFrom(typeof (IGrainWithStringKey)))
                {
                    return typeof (string);
                }
            }
            return null;
        }

        private static MethodInfo ReflectGetGrainMethod(Type grainKeyType, Type grainInterface)
        {
            return typeof(GrainFactory)
                .GetMethod("GetGrain", new[] { grainKeyType, typeof(string) })
                .MakeGenericMethod(grainInterface);
        }
    }
}