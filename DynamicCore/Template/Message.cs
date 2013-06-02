using System;
using System.Collections.Generic;

namespace LINQBridge.DynamicCore.Template
{
    [Serializable]
    internal class Message
    {
        public string FileName;
        public string TypeFullName;
        public string TypeLocation;
        public string TypeNamespace;
        public string AssemblyQualifiedName;
        public List<string> ReferencedAssemblies;

        public Message()
        {
            ReferencedAssemblies = new List<string>();
        }

        public override string ToString()
        {
            return FileName + " " + TypeFullName + " " + TypeLocation + " " + TypeNamespace;
        }
    }
}
