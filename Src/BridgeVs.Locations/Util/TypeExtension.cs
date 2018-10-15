using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BridgeVs.Shared.Util
{
    public static class TypeExtension
    {
        public static List<string> FindAssemblyNames(this Type targetType)
        {
            string currentAssemblyName = targetType.Assembly.GetName().Name;
            bool IsSystemAssembly(string name) => name.Contains("Microsoft") || name.Contains("System") || name.Contains("mscorlib");

            if (targetType.IsGenericType)
            {
                var assNames = (from genericType in targetType.GetGenericArguments()
                                let name = genericType.Assembly.GetName().Name
                                where !IsSystemAssembly(name)
                                select name).Distinct();
                return assNames.ToList();
            }

            return new List<string> { currentAssemblyName };
        }
    }
}
