#region License
// Copyright (c) 2013 Giovanni Campo
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

using Grapple;
using LINQBridgeVs.DynamicCore.Helper;
using LINQBridgeVs.DynamicCore.Template;
using LINQBridgeVs.Logging;
using Microsoft.Win32;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace LINQBridgeVs.DynamicCore
{
    public static class DynamicObjectSource
    {
        internal const string FileNameFormat = "{0}.linq";

        public static void BroadCastData(object target, Stream outgoingData, string vsVersion)
        {
            Log.Configure("LINQBridgeVs", "DynamicCore");

            Log.Write("Vs Targeted Version ", vsVersion);
            try
            {
                var targetType = GetInterfaceTypeIfIsIterator(target);
                var targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, true);
                var targetTypeName = TypeNameHelper.GetDisplayName(targetType, false);
                //I'm lazy I know it...
                var pattern1 = new Regex("[<]");
                var pattern2 = new Regex("[>]");
                var pattern3 = new Regex("[,]");
                var pattern4 = new Regex("[`]");
                var pattern5 = new Regex("[ ]");

                var fileName = pattern1.Replace(targetTypeFullName, "(");
                fileName = pattern2.Replace(fileName, ")");

                var typeName = pattern1.Replace(targetTypeName, string.Empty);
                typeName = pattern2.Replace(typeName, string.Empty);
                typeName = pattern3.Replace(typeName, string.Empty);
                typeName = pattern4.Replace(typeName, string.Empty);
                typeName = pattern5.Replace(typeName, string.Empty);

                fileName = TypeNameHelper.RemoveSystemNamespaces(fileName);

                var message = new Message
                {
                    FileName = string.Format(FileNameFormat, fileName),
                    TypeName = typeName.Trim(),
                    TypeFullName = targetTypeFullName,
                    //  TypeLocation = targetType.Assembly.Location, //This is to be changed to read that value off the Registry...
                    TypeLocation = GetAssemblyLocation(vsVersion, targetType.Name),
                    TypeNamespace = targetType.Namespace,
                    AssemblyQualifiedName = targetType.AssemblyQualifiedName
                };

                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(outgoingData, message);

                Log.Write("BroadCastData to LINQBridgeVsTruck");

                var truck = new Truck("LINQBridgeVsTruck");
                truck.LoadCargo(target);
                var res = truck.DeliverTo(typeName);
                Log.Write("Data Succesfully Shipped to Grapple");

            }
            catch (Exception e)
            {
                Log.Write(e, "Error in BroadCastData");
            }
        }

        private static Type GetInterfaceTypeIfIsIterator(object o)
        {
            Log.Write("GetInterfaceTypeIfIsIterator Started");
            var @type = o.GetType();

            if (!@type.IsNestedPrivate || !@type.Name.Contains("Iterator") ||
                !@type.FullName.Contains("System.Linq.Enumerable") || !(o is IEnumerable)) return @type;

            if (@type.BaseType == null || @type.BaseType.FullName.Contains("Object"))
                return @type.GetInterface("IEnumerable`1");

            Log.Write("Iterator type, LINQ Query found {0}", @type.BaseType.ToString());
            return @type.BaseType.GetInterface("IEnumerable`1");
        }

        /// <summary>
        /// Gets the assembly location. If an assembly is loaded at Runtime or it's loaded within a IIS context Assembly.Location property is null
        /// </summary>
        /// <param name="vsVersion">The Visual Studio version.</param>
        /// <param name="assemblyName">Name of the assembly to search in the registry</param>
        /// <returns></returns>
        private static string GetAssemblyLocation(string vsVersion, string assemblyName)
        {
            var registryKeyPath = string.Format(@"Software\LINQBridgeVs\{0}\EnabledProjects", vsVersion);

            using (var key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
            {
                var value = key.GetSubKeyNames();
                foreach (var values in from element in value
                                       select key.OpenSubKey(element)
                                           into subKey
                                           let name = subKey.GetValueNames().FirstOrDefault(p => p == assemblyName)
                                           where name != null
                                           select (string[])subKey.GetValue(name))
                {
                    Log.Write("Assembly Location Found: ", values[1]);
                    return values[1]; //At Position 1 there's the Assembly Path previously saved (When project was initially LINQBridged)
                }
            }
            Log.Write("Assembly Location Found None");

            return null;
        }
    }
}
