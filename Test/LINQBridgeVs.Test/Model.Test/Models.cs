using System;
using System.Collections.Generic;
using AnotherModel.UnitTest;

namespace Model.UnitTest
{
    [Serializable]
    public class CustomType1
    {
        public string SField1;
        public int IntField2;
        public List<AnotherModelTest> AnotherModelList;
        public IEnumerable<string> AnotherEnumerable;

        public CustomType1()
        {
            AnotherModelList = new List<AnotherModelTest> { new AnotherModelTest() };
            AnotherEnumerable = new List<string> {"String1", "String2"};
        }
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

    public class CustomType4 : Dictionary<string, AnotherModelTest>
    {
        

    }

    [Serializable]
    public class VisualizationTestClass
    {
        public CustomType1 CustomType1;

        public VisualizationTestClass( )
        {
            CustomType1 = new CustomType1();
        }
    }
}
