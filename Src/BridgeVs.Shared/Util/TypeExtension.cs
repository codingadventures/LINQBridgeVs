using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeVs.Shared.Util
{
    public static class TypeExtension
    {
        public static List<string> FindAssemblyNames(this Type targetType)
        {
            string currentAssemblyName = targetType.Assembly.GetName().Name;

            if (!targetType.IsGenericType)
            {
                return new List<string> { currentAssemblyName };
            }

            IEnumerable<string> assNames = from genericType in targetType.GetGenericArguments()
                                            let name = genericType.Assembly.GetName().Name
                                            select name;

            return assNames.Distinct().ToList();
        }
    }
}
