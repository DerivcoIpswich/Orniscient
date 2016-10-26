using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Only load the assemblies that we can load. 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly,Logger logger)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.Warn(1,$"Could not load all types for assembly : {assembly.FullName}",e);
                return e.Types.Where(t => t != null);
            }
        }
    }
}