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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BridgeVs.Grapple.UnitTest
{
    [Serializable]
    public class BinaryTestModel
    {
        private static readonly Random Random = new Random();

        public List<int> Ints = new List<int>();
        public Dictionary<string, string> Dictionary = new Dictionary<string, string>();
        public BinaryTestStruct BinaryTestStruct { get; set; }
        public BinaryTestModel()
        {
            BinaryTestStruct = new BinaryTestStruct { Type = Ints.GetType().FullName };
            for (int i = 0; i < 100; i++)
            {
                Ints.Add(Random.Next(0, int.MaxValue));
                Dictionary.Add(i.ToString(CultureInfo.InvariantCulture), Ints[i].ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    [Serializable]
    public struct BinaryTestStruct
    {
        public string Type { get; set; }
    } 
    
 
    public class BsonTestModel
    {
        private static readonly Random Random = new Random();

        public List<int> Ints = new List<int>();
        public Dictionary<string, string> Dictionary = new Dictionary<string, string>();
        public BinaryTestStruct BinaryTestStruct { get; set; }
        public BsonTestModel()
        {
            BinaryTestStruct = new BinaryTestStruct { Type = Ints.GetType().FullName };
            for (int i = 0; i < 100; i++)
            {
                Ints.Add(Random.Next(0, int.MaxValue));
                Dictionary.Add(i.ToString(CultureInfo.InvariantCulture), Ints[i].ToString(CultureInfo.InvariantCulture));
            }
        }
    }

 
    public struct BsonTestStruct
    {
        public string Type { get; set; }
    }
}
