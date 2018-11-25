using System;
using System.Linq;
using System.Reflection;

namespace BridgeVs.UnitTest
{
    // Mark as MarshalByRefObject allows method calls to be proxied across app-domain boundaries
    public class PluginRunner : MarshalByRefObject
    {
        public Assembly Assembly { get; private set; }
        // make sure that we're loading the assembly into the correct app domain.
        public void LoadAssembly(byte[] byteArr)
        {
            Assembly = Assembly.Load(byteArr);
        }

        // be careful here, only types from currently loaded assemblies can be passed as parameters / return value.
        // also, all parameters / return values from this object must be marked [Serializable]
        public string CreateAndExecutePluginResult(string assemblyQualifiedTypeName)
        {
            var domain = AppDomain.CurrentDomain;

            // we use this overload of GetType which allows us to pass in a custom AssemblyResolve function
            // this allows us to get a Type reference without searching the disk for an assembly.
            var pluginType = Type.GetType(
                assemblyQualifiedTypeName,
                (name) => domain.GetAssemblies().Where(a => a.FullName == name.FullName).FirstOrDefault(),
                null,
                true);

            dynamic plugin = Activator.CreateInstance(pluginType);

            // do whatever you want here with the instantiated plugin
            string result = plugin.RunTest();

            // remember, you can only return types which are already loaded in the primary app domain and can be serialized.
            return result;
        }
    }
}
