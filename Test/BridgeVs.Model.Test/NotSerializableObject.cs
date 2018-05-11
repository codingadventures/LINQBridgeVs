#region License
// Copyright (c) 2013 Coding Adventures
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


using System.Collections;
using System.Collections.Generic;


namespace BridgeVs.Model.Test
{
    public interface INotSerializable
    {
        void Do();
    }
    public class AClass : INotSerializable
    {
        public void Do()
        {
   
        }
    }

    public enum Enum1
    {
        A, B, C
    }

    public struct RandomStruct
    {
        public bool Check;
        public Hashtable Map { get; set; }
    }
   
    public class NotSerializableObject
    {
        public Enum1 Prop1 { get; set; }
        public NotSerializableList NotSerializableClass { get; set; }
        public int Int { get; set; }
        public Dictionary<string, RandomStruct> Dictionary { get; set; }
        public INotSerializable NotSerializable { get; set; }


        public NotSerializableObject()
        {
            var r = new RandomStruct { Check = true, Map = new Hashtable { { 123312, new List<int> { 1, 2, 2, 3 } } } };

            Prop1 = Enum1.B;
            Int = 123321;
            Dictionary = new Dictionary<string, RandomStruct>
                             {
                              {"1",r }   
                             };
            NotSerializable = new AClass();

        }
    }
}
