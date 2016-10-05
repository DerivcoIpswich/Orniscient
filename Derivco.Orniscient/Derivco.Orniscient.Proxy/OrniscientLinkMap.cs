using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy
{
    public class OrniscientLinkMap : IOrniscientLinkMap
    {
        private static readonly Lazy<OrniscientLinkMap> _instance = new Lazy<OrniscientLinkMap>(() => new OrniscientLinkMap());
        private Dictionary<Type, Attributes.OrniscientGrain> _typeMap;

        private OrniscientLinkMap()
        {

            if (_typeMap == null)
            {
                CreateTypeMap();
            }
        }


        private void CreateTypeMap()
        {
            _typeMap = new Dictionary<Type, OrniscientGrain>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetLoadableTypes())
                {
                    //if this type is not a grain we do not want it..
                    if (!typeof(IGrain).IsAssignableFrom(type))
                        continue;

                    var attribs = type.GetCustomAttributes(typeof(Attributes.OrniscientGrain), false);
                    var orniscientInfo = attribs.FirstOrDefault() as OrniscientGrain ?? new OrniscientGrain();
                    orniscientInfo.IdentityType = GetIdentityType(type);

                    if (orniscientInfo.HasLinkFromType && string.IsNullOrEmpty(orniscientInfo.DefaultLinkFromTypeId))
                    {
                        orniscientInfo.DefaultLinkFromTypeId = GetDefaultLinkFromTypeId(type);
                    }
                    _typeMap.Add(type, orniscientInfo);
                }
            }
        }



        private IdentityTypes GetIdentityType(Type type)
        {
            if (typeof(IGrainWithGuidKey).IsAssignableFrom(type))
            {
                return IdentityTypes.Guid;
            }
            else if (typeof(IGrainWithIntegerKey).IsAssignableFrom(type))
            {
                return IdentityTypes.Int;

            }
            else if (typeof(IGrainWithStringKey).IsAssignableFrom(type))
            {
                return IdentityTypes.String;
            }
            return IdentityTypes.NotFound;
        }

        private string GetDefaultLinkFromTypeId(Type type)
        {
            if (typeof(IGrainWithGuidKey).IsAssignableFrom(type))
            {
                return Guid.Empty.ToString();
            }
            else if (typeof(IGrainWithIntegerKey).IsAssignableFrom(type))
            {
                return "0";
            }
            return string.Empty;
        }

        public static OrniscientLinkMap Instance => _instance.Value;

        public OrniscientGrain GetLinkFromType(string type)
        {
            return GetLinkFromType(GetType(type)) ?? new OrniscientGrain();
        }

        public OrniscientGrain GetLinkFromType(Type type)
        {
            if (type == null) return null;
            return _typeMap.ContainsKey(type) ? _typeMap[type] : null;
        }

        private Type GetType(string typeName)
        {
            var temp = AppDomain.CurrentDomain.GetAssemblies();
            return temp.Select(a => a.GetType(typeName)).FirstOrDefault(t => t != null);
        }
    }

    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
