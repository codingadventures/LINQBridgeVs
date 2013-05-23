using System;
using System.Collections.Generic;

namespace LINQBridge.DynamicVisualizers.Template
{
    [Serializable]
    internal struct Message
    {
        public string FileName;
        public string TypeFullName;
        public string TypeLocation;
        public string TypeNamespace;
        public List<string> ReferencedAssemblies;

        public override string ToString()
        {
            return FileName + " " + TypeFullName + " " + TypeLocation + " " + TypeNamespace;
        }
    }
}
