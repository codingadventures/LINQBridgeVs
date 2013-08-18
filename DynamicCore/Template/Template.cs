using System.Collections.Generic;
using LINQBridge.DynamicCore.Utils;

namespace LINQBridge.DynamicCore.Template
{
    public partial class Inspection
    {
        private readonly string _typeNamespace;

        private readonly List<string> _assemblies;
        private readonly string _typeToRetrieveFullName;

        public Inspection(List<string> assemblies, string typeToRetrieveFullName, string typeNamespace)
        {
            _typeNamespace = typeNamespace;
            _assemblies = assemblies;
            _typeToRetrieveFullName = TypeNameHelper.RemoveSystemNamespaces(typeToRetrieveFullName);
        }
    }
}
