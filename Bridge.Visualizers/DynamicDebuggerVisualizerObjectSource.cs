using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Linq;
using LINQBridge.DynamicVisualizers.Template;
using LINQBridge.DynamicVisualizers.Utils;
using LINQBridge.Grapple;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace LINQBridge.DynamicVisualizers
{

    public class DynamicDebuggerVisualizerObjectSource : VisualizerObjectSource
    {
        internal const string FileNameFormat = "ddMMyy_{0}.linq";

        private static readonly List<string> PublicExcludedKey = new List<string> { "b77a5c561934e089", "b03f5f7f11d50a3a", "31bf3856ad364e35" };

        public override void GetData(object target, Stream outgoingData)
        {
            var scriptFileName = DateTime.Now.ToString(FileNameFormat);
            var targetType = target.GetType();
            var targetTypeFullName = TypeNameHelper.GetDisplayName(targetType, true);
            var pattern = new Regex("[<>]");


            var referencedAssemblies = targetType.Assembly.GetReferencedAssemblies()
                .Where(name => !PublicExcludedKey.Contains(GetAssemblyPublickKeyToken(name)))
                .Where(name => !name.Name.Contains("Microsoft") || !name.Name.Contains("System"))
                .Select(name => Assembly.ReflectionOnlyLoad(name.FullName).Location);

            var message = new Message
                              {
                                  FileName = string.Format(scriptFileName, pattern.Replace(targetTypeFullName, string.Empty)),
                                  TypeFullName = targetTypeFullName,
                                  TypeLocation = targetType.Assembly.Location,
                                  TypeNamespace = targetType.Namespace,
                                  ReferencedAssemblies = referencedAssemblies.ToList()
                              };

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(outgoingData, message);


            var busChannel = Bus.Instance;
            busChannel.Add(target);
            busChannel.BroadCast();

        }

        /// <summary>  
        /// Gets the public key token of the supplied argument  
        /// </summary>  
        /// <param name="assembly">The <see cref="Assembly">to get the public key token for</param>  
        /// <returns></returns>  
        /// <exception cref="ArgumentNullException">If <paramref name="assembly">is null.</exception>  
        public static string GetAssemblyPublickKeyToken(AssemblyName assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            byte[] token = assembly.GetPublicKeyToken();
            if (token == null || token.Length == 0)
                return null;
            return token.Select(x => x.ToString("x2")).Aggregate((x, y) => x + y);
        }
    }
}
