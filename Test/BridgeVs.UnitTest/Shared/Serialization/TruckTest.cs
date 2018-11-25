#region License
// Copyright (c) 2013 -2018 Coding Adventures
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
// NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BridgeVs.Shared.Options;
using BridgeVs.Shared.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.UnitTest.Shared.Serialization
{
    internal class NonSerializableClass
    {
        public string AString = "I'm Not Serializable";

    }
    [Serializable]
    public struct BinaryTestStruct
    {
        public string Type { get; set; }
    }

    [TestClass]
    public class TruckTest
    {
        private static readonly Dictionary<string, List<int>> KeyValues
            = new Dictionary<string, List<int>> { { "1", new List<int> { 1, 3, 2, 1, 1 } } };

        private readonly List<int> _intList = new List<int> { 123, 12, 312, 312, 3 };

        private readonly List<string> _stringList = new List<string> { "asdasd", "asdasd123123" };

        private readonly LinkedList<string> _linkedList = new LinkedList<string>();

        public TruckTest()
        {
            _linkedList.AddFirst(new LinkedListNode<string>("Hello"));
            _linkedList.AddLast(new LinkedListNode<string>("World!"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TruckJsonSerializerTest_Array_of_Objects()
        {
            string truckId = Guid.NewGuid().ToString();
            object[] test =  { "test test test" };

            SerializationOption? serializationOption = Truck.SendCargo(test, truckId, SerializationOption.JsonSerializer );
           
            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");
            Assert.IsTrue(serializationOption.Value == SerializationOption.JsonSerializer, "Truck.SendCargo used Binary Serialization instead");

            object[] objects = Truck.ReceiveCargo<object[]>(truckId, serializationOption.Value);

            Assert.IsNotNull(objects); 
            Assert.AreEqual(1, objects.Length);
            Assert.AreEqual(objects[0], test[0]);
        }
         

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TruckJsonSerializerTest_Dictionary()
        {
            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption =  Truck.SendCargo(KeyValues, truckId, SerializationOption.JsonSerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            Dictionary<string, List<int>> retValue = Truck.ReceiveCargo<Dictionary<string, List<int>>>(truckId, serializationOption.Value);

            Assert.IsNotNull(retValue);
            Assert.AreNotSame( KeyValues, retValue, "The two objects are identical - same memory footprint");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AddNonSerializableObjectTest_BinarySerializer()
        {
            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(new NonSerializableClass(), truckId, SerializationOption.BinarySerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            NonSerializableClass nonSerializableClass = Truck.ReceiveCargo<NonSerializableClass>(truckId, serializationOption.Value);

            Assert.AreEqual("I'm Not Serializable", nonSerializableClass.AString);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AddNonSerializableObjectTest_JsonSerializer_ByType()
        {
            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(new NonSerializableClass(), truckId, SerializationOption.BinarySerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            Assert.IsTrue(serializationOption.Value == SerializationOption.JsonSerializer, "Serialization option is not JsonSerializer over non serializable type");

            NonSerializableClass nonSerializableClass = Truck.ReceiveCargo(truckId, serializationOption.Value, typeof(NonSerializableClass)) as NonSerializableClass;
            Assert.IsNotNull(nonSerializableClass, "Serialization has failed");
            Assert.AreEqual("I'm Not Serializable", nonSerializableClass.AString);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AddNonSerializableObjectTest_JsonSerializer()
        {
            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(new NonSerializableClass(), truckId, SerializationOption.JsonSerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            NonSerializableClass nonSerializableClass = Truck.ReceiveCargo<NonSerializableClass>(truckId, serializationOption.Value);

            Assert.AreEqual("I'm Not Serializable", nonSerializableClass.AString);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AddIteratorTypeSelectWhereIteratorOfIntShouldSerialize_BinarySerializer()
        {
            IEnumerable<int> r = from i in Enumerable.Range(0, 100)
                                 select i;

            string truckId = Guid.NewGuid().ToString();

            SerializationOption? serializationOption = Truck.SendCargo(r.ToList(), truckId, SerializationOption.BinarySerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            Assert.IsTrue(serializationOption.Value == SerializationOption.BinarySerializer, "Serialization option is not JsonSerializer over non serializable type");

            object res = Truck.ReceiveCargo(truckId, serializationOption.Value, typeof(IEnumerable<int>));

            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(IEnumerable<int>));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AddIteratorTypeSelectIteratorOfCustomStructShouldSerialize()
        {

            IEnumerable<BinaryTestStruct> r = from i in Enumerable.Range(0, 100)
                                              select new BinaryTestStruct { Type = i.ToString(CultureInfo.InvariantCulture) };

            string truckId = Guid.NewGuid().ToString();

            SerializationOption? serializationOption = Truck.SendCargo(r.ToList(), truckId, SerializationOption.BinarySerializer);

            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            Assert.IsTrue(serializationOption.Value == SerializationOption.BinarySerializer, "Serialization option is not JsonSerializer over serializable type");

            IEnumerable<BinaryTestStruct> res = Truck.ReceiveCargo<IEnumerable<BinaryTestStruct>>(truckId, serializationOption.Value);
             
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(IEnumerable<BinaryTestStruct>));

        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Verify_List_String_Is_Not_Null_And_Has_2_Elements()
        {

            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(_stringList, truckId, SerializationOption.BinarySerializer);
            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            List<string> ret = Truck.ReceiveCargo<List<string>>(truckId, serializationOption.Value);

            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 2);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Verify_List_Int_Is_Not_Null_And_Has_5_Elements()
        {
            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(_intList, truckId, SerializationOption.BinarySerializer);
            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            List<int> ret = Truck.ReceiveCargo<List<int>>(truckId, serializationOption.Value);
            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 5);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Verify_LinkedList_String_Is_Not_Null_And_Has_2_Elements()
        {
           

            string truckId = Guid.NewGuid().ToString();
            SerializationOption? serializationOption = Truck.SendCargo(_linkedList, truckId, SerializationOption.BinarySerializer);
            Assert.IsTrue(serializationOption.HasValue, "Truck.SendCargo returned null serialization option");

            LinkedList<string> ret = Truck.ReceiveCargo<LinkedList<string>>(truckId, serializationOption.Value);
            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 2);
        }
    }
}