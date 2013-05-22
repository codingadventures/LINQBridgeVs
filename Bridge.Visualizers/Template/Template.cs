using System.Collections.Generic;

namespace LINQBridge.DynamicVisualizers.Template
{
    public partial class Inspection
    {
        public readonly string TypeNamespace;

        public readonly List<string> Assemblies;
        public readonly string TypeToRetrieveFullName;

        public Inspection(List<string> assemblies, string typeToRetrieveFullName, string typeNamespace)
        {
            TypeNamespace = typeNamespace;
            Assemblies = assemblies;
            TypeToRetrieveFullName = typeToRetrieveFullName;
        }
    }
}
