using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Derivco.Orniscient.Proxy
{
    public class OrniscientLinkMap
    {
        private static readonly Lazy<OrniscientLinkMap> _instance = new Lazy<OrniscientLinkMap>(() => new OrniscientLinkMap());
        private Dictionary<Type, Attributes.Orniscient> _typeMap;
        Dictionary<string, Type> typeCache;

        private OrniscientLinkMap()
        {
            if (_typeMap == null)
            {
                createTypeMap();
            }
        }

        private void createTypeMap()
        {
            _typeMap = new Dictionary<Type, Attributes.Orniscient>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(Attributes.Orniscient), false);
                    if (attribs.Length <= 0) continue;

                    var linkFromType = attribs.First() as Attributes.Orniscient;
                    if (linkFromType != null && linkFromType.HasLinkFromType)
                        _typeMap.Add(type, linkFromType);
                }
            }
        }

        public static OrniscientLinkMap Instance => _instance.Value;

        public Attributes.Orniscient GetLinkFromType(string type)
        {
            return GetLinkFromType(GetType(type));
        }

        public Attributes.Orniscient GetLinkFromType(Type type)
        {
            if (type == null) return null;
            return _typeMap.ContainsKey(type) ? _typeMap[type] : null;
        }

        private Type GetType(string typeName)
        {
            var temp = AppDomain.CurrentDomain.GetAssemblies();
            //TODO : Cache this info somehow
            foreach (var a in temp)
            {
                var t = a.GetType(typeName);
                if (t != null)
                    return t;
            }
            return null;
        }
    }
}
