using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Test.AssemblyModel
{
    public class CustomType1
    {
        public string SField1;
        public int IntField2;
    }

    public class CustomType2
    {
        public bool BField1;

        public double DProp1 { get; set; }
    }

    public class CustomType3
    {
        public Int64 Field1;

        public List<int?> Prop1 { get; set; }
    }

    [Serializable]
    public class VisualizationTestClass
    {
        
    }
}
