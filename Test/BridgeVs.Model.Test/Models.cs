#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using BridgeVs.AnotherModel.Test;

namespace BridgeVs.Model.Test
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
