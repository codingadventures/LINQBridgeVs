#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using Bridge.Logging;

namespace LINQBridgeVs.DynamicCore.Helper
{
    internal static class ReferencedAssemblyFinderHelper
    {
        internal static List<string> GetReferencedAssemblies(this Type @type, string originalTypeLocation)
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
                    if (!string.IsNullOrEmpty(originalTypeLocation))
                        returnList.Add(originalTypeLocation);

                    Log.WriteIf(
                        !string.IsNullOrEmpty(originalTypeLocation) &&
                        !@type.Assembly.Location.Equals(originalTypeLocation), "No Referenced Assemblies");

                    var referencedAssemblyPaths = @type.Assembly.GetReferencedAssembliesPath(originalTypeLocation);

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
