using System;
using System.Collections.Generic;

namespace Bridge.Visualizers.Template
{
    public partial class Inspection
    {

        public readonly List<string> Assemblies;
        public readonly Type TypeToRetrieve;

        public Inspection(List<string> assemblies, Type typeToRetrieve)
        {
            Assemblies = assemblies;
            TypeToRetrieve = typeToRetrieve;
        }
    }
}
