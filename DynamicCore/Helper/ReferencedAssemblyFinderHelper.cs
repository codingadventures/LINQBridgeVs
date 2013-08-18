using System;
using System.Collections.Generic;
using System.Linq;
using LINQBridge.Logging;

namespace LINQBridge.DynamicCore.Helper
{
    internal static class ReferencedAssemblyFinderHelper
    {

        internal static List<string> GetReferencedAssemblies(this Type @type, string originalTypeLocation = null)
        {
            var returnList = new List<string>();
            if (@type == null) return returnList;

            try
            {
                if (@type.IsGenericType)
                {
                    var genericTypes = @type.GetGenericArguments();
                    foreach (var genericType in genericTypes)
                    {
                        Log.Write("Generic Type Found {0}: ", genericType);

                        returnList.AddRange(GetReferencedAssemblies(genericType, originalTypeLocation));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(originalTypeLocation) && originalTypeLocation != @type.Assembly.Location)
                        returnList.Add(@type.Assembly.Location);

                    Log.WriteIf(
                        !string.IsNullOrEmpty(originalTypeLocation) &&
                        !@type.Assembly.Location.Equals(originalTypeLocation), "No Referenced Assemblies");

                    var referencedAssemblyPaths = @type.Assembly.GetReferencedAssembliesPath();

                    returnList.AddRange(referencedAssemblyPaths);
                }
            }
            catch (Exception e)
            {
                Log.Write(e, "Error While getting the Referenced Assemblies");

                throw;
            }

            return returnList;

        }
    }
}
