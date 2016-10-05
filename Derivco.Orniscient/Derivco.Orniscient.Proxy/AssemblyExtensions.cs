using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Derivco.Orniscient.Proxy
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Only load the assemblies that we can load. 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
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