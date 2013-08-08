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
        public readonly List<string> ReferencedAssemblies;

        public Message()
        {
            ReferencedAssemblies = new List<string>(30);
        }

        public override string ToString()
        {
            return "FileName: " + FileName 
                + Environment.NewLine 
                + "TypeFullName: " + TypeFullName 
                + Environment.NewLine 
                + "TypeLocation: " + TypeLocation 
                + Environment.NewLine
                + "TypeNamespace" + TypeNamespace;
        }
    }
}
