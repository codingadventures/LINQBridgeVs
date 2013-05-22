using System;

namespace LINQBridge.DynamicVisualizers.Template
{
    [Serializable]
    internal struct Message
    {
        public string FileName;
        public string TypeFullName;
        public string TypeLocation;
        public string TypeNamespace;

        public override string ToString()
        {
            return FileName + " " + TypeFullName + " " + TypeLocation + " " + TypeNamespace;
        }
    }
}
