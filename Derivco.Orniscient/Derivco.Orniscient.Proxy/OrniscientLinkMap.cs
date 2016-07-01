using System;
using System.Collections.Generic;
using System.Linq;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;

namespace Derivco.Orniscient.Proxy
{
    public class OrniscientLinkMap
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
            _typeMap = new Dictionary<Type, Attributes.OrniscientGrain>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof (Attributes.OrniscientGrain), false);
                    if (attribs.Length <= 0) continue;

                    var orniscientInfo = attribs.First() as Attributes.OrniscientGrain;

                    //if (string.IsNullOrEmpty(orniscientInfo.DefaultLinkFromTypeId))
                    //{
                        if (typeof (IGrainWithGuidKey).IsAssignableFrom(type))
                        {
                            orniscientInfo.IdentityType = IdentityTypes.Guid;
                            orniscientInfo.DefaultLinkFromTypeId = Guid.Empty.ToString();
                        }
                        else if (typeof (IGrainWithIntegerKey).IsAssignableFrom(type))
                        {
                            orniscientInfo.IdentityType = IdentityTypes.Int;
                            orniscientInfo.DefaultLinkFromTypeId = "0";
                        }else if (typeof (IGrainWithStringKey).IsAssignableFrom(type))
                        {
                            orniscientInfo.IdentityType = IdentityTypes.String;
                        }
                    //}

                    _typeMap.Add(type, orniscientInfo);
                    //if (linkFromType != null && linkFromType.HasLinkFromType)
                    //    _typeMap.Add(type, linkFromType);

                }
            }
        }

        public static OrniscientLinkMap Instance => _instance.Value;

        public Attributes.OrniscientGrain GetLinkFromType(string type)
        {
            return GetLinkFromType(GetType(type));
        }

        public Attributes.OrniscientGrain GetLinkFromType(Type type)
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
}
