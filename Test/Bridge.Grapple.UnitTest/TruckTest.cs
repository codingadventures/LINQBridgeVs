#region License
// Copyright (c) 2013 Giovanni Campo
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


using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Grapple.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grapple.UnitTest
{
    internal class NonSerializableClass
    {
        public string AString = "I'm Not Serializable";

    }
    [TestClass]
    public class TruckTest
    {
        private static ITruck _truck;
        private static readonly Dictionary<string, List<int>> KeyValues
            = new Dictionary<string, List<int>> { { "1", new List<int> { 1, 3, 2, 1, 1 } } };

        [TestInitialize]
        public void Init()
        {
            _truck = new Truck("Iveco");
        }

        [TestMethod]
        public void TruckConstructorTest()
        {
            var b = _truck;

            Assert.IsNotNull(b, "Error initializing the object with the Default Instance (See Singleton)");
            Assert.IsInstanceOfType(b, typeof(Truck));
            Assert.AreSame(_truck, b, "Object must be the same istance, Check if object has been instanciated correctly");
        }


        [TestMethod]
        public void TruckLoadCargoTest()
        {
            const string test = "test test test";
            _truck.LoadCargo(test);
            _truck.LoadCargo(test);

            var objects = _truck.UnStuffCargo<string>();

            Assert.IsNotNull(objects);
            IEnumerable<string> enumerable = objects as IList<string> ?? objects.ToList();
            Assert.AreEqual(2, enumerable.Count());
            Assert.AreEqual(enumerable.First(), test);

        }

        [TestMethod]
        public void TruckDeliverToTest()
        {
            _truck.LoadCargo(KeyValues);
            _truck.DeliverTo("Test");

        }

        [TestMethod]
        public void TruckUnStuffCargoTest()
        {
            _truck.LoadCargo(KeyValues);

            var retValue = _truck.UnStuffCargo<Dictionary<string, List<int>>>();

            Assert.IsNotNull(retValue);
            var enumerable = retValue as IList<Dictionary<string, List<int>>> ?? retValue.ToList();

            Assert.IsTrue(enumerable.Count() == 1);
            Assert.AreNotSame(enumerable.First(), KeyValues);

        }

        [TestMethod]
        public void AddNonSerializableObjectTest()
        {
            _truck.LoadCargo(new NonSerializableClass());
            _truck.DeliverTo("MaybeMom");
            _truck.WaitDelivery("MaybeMom").Wait();
            var nonSerializableClass = _truck.UnStuffCargo<NonSerializableClass>().FirstOrDefault();

            Assert.AreEqual("I'm Not Serializable", nonSerializableClass.AString);
        }

        [TestMethod]
        public void AddNonSerializableObjectTestRetrieveByType()
        {
            _truck.LoadCargo(new NonSerializableClass());
            _truck.DeliverTo("MaybeMom");
            _truck.WaitDelivery("MaybeMom").Wait();
            var nonSerializableClass = (NonSerializableClass)_truck.UnStuffCargo(typeof(NonSerializableClass)).FirstOrDefault();
            Assert.AreEqual("I'm Not Serializable", nonSerializableClass.AString);
        }

        [TestMethod]
        public void AddIteratorTypeSelectWhereIteratorOfIntShouldSerialize()
        {
            var r = from i in Enumerable.Range(0, 100)
                    select i;
            _truck.LoadCargo(r);
            _truck.LoadCargo(r.GetType());
            _truck.DeliverTo("Mom");
            _truck.WaitDelivery("Mom").Wait();
            var res = _truck.UnStuffCargo<IEnumerable<int>>().FirstOrDefault();

            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(IEnumerable<int>));

        }

        [TestMethod]
        public void AddIteratorTypeSelectWhereIteratorOfCustomStructShouldSerialize()
        {

            var r = from i in Enumerable.Range(0, 100)
                    select new BinaryTestStruct { Type = i.ToString(CultureInfo.InvariantCulture) };

            _truck.LoadCargo(r);
            _truck.LoadCargo(r.GetType());
            _truck.DeliverTo("Mom");
            _truck.WaitDelivery("Mom").Wait();
            var res = _truck.UnStuffCargo<IEnumerable<BinaryTestStruct>>().FirstOrDefault();

            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(IEnumerable<BinaryTestStruct>));

        }

        [TestMethod]
        public void AddIteratorTypeSelectWhereIteratorOfObjectShouldSerialize()
        {

            var r = from i in Enumerable.Range(0, 100)
                    select new BinaryTestStruct { Type = i.ToString(CultureInfo.InvariantCulture) };

            _truck.LoadCargo((object)r);

            _truck.DeliverTo("Mom");
            _truck.WaitDelivery("Mom").Wait();
            var res = _truck.UnStuffCargo<IEnumerable<BinaryTestStruct>>().FirstOrDefault();

            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(IEnumerable<BinaryTestStruct>));

        }

        [TestMethod]
        public void WaitForNotExistingTruckWaitForAtLeast5000Milliseconds()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            _truck.WaitDelivery("SomeRandomAddressToWaitFor").Wait();
            stopWatch.Stop();

            Assert.IsTrue(stopWatch.ElapsedMilliseconds >= 5000);

        }
    }
}
