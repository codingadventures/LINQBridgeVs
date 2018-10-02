#region License
// Copyright (c) 2013 - 2018 Coding Adventures
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
using BridgeVs.Shared.Logging;

namespace BridgeVs.DynamicVisualizers.Helper
{
    internal static class ReferencedAssemblyFinderHelper
    {
        internal static List<string> GetReferencedAssemblies(this Type @type, string originalTypeLocation)
        {
            List<string> returnList = new List<string>();
            if (@type == null)
                return returnList;

            try
            {
                if (@type.IsGenericType)
                {
                    Type[] genericTypes = @type.GetGenericArguments();
                    foreach (Type genericType in genericTypes)
                    {
                        Log.Write($"Generic Type Found {genericType}");
                        returnList.AddRange(GetReferencedAssemblies(genericType, originalTypeLocation));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(originalTypeLocation))
                        returnList.Add(originalTypeLocation);

                    if (!string.IsNullOrEmpty(originalTypeLocation) &&
                        !@type.Assembly.Location.Equals(originalTypeLocation))
                        Log.Write("No Referenced Assemblies");

                    IEnumerable<string> referencedAssemblyPaths = @type.Assembly.GetReferencedAssembliesPath(originalTypeLocation);

                    returnList.AddRange(referencedAssemblyPaths);
                }
            }
            catch (Exception e)
            {
                Log.Write(e, "Error While getting the Referenced Assemblies");
                string vsVersion = VisualStudioVersionHelper.FindCurrentVisualStudioVersion();

                e.Capture(vsVersion, message: "Error While getting the Referenced Assemblies");

                throw;
            }

            return returnList;
        }


    }
}
