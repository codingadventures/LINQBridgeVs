using System;
 

namespace Bridge.Visualizers.Template
{
    [Serializable]
    internal struct  Message
    {
        public string FileName;
        public string TypeFullName;
        public string TypeLocation;
        public string TypeNamespace;
    }
}
