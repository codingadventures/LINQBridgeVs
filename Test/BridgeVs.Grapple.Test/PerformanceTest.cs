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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.Grapple.Test
{
    [TestClass]
    public class PerformanceTest
    {
        private static readonly ConcurrentBag<BinaryTestModel> BinaryTestModels = new ConcurrentBag<BinaryTestModel>( );
        private static readonly ConcurrentBag<BsonTestModel> BsonTestModels = new ConcurrentBag<BsonTestModel>( );
        [ClassInitialize]
        public static void Init(TestContext ctx)
        {

            double upper = Math.Pow(10, 3);
            const int i = 0;
            Parallel.For(i, (int)upper, index => BinaryTestModels.Add(new BinaryTestModel()));
            Parallel.For(i, (int)upper, index => BsonTestModels.Add(new BsonTestModel()));
        }

        [TestMethod]
        public void BinaryStressTest()
        {
            Truck b = new Truck("MAN");

            double upper = Math.Pow(10, 3);

            Stopwatch sw = new Stopwatch();

            sw.Start();
             
            b.LoadCargo(BinaryTestModels.ToList());
            
            sw.Stop();

            long secondElapsedToAdd = sw.ElapsedMilliseconds;

            Trace.WriteLine(string.Format("Put on the Channel {1} items. Time Elapsed: {0}", secondElapsedToAdd, upper));
            sw.Reset();
            sw.Start();

            b.DeliverTo("Dad");
            sw.Stop();

            long secondElapsedToBroadcast = sw.ElapsedMilliseconds ;

            Trace.WriteLine(string.Format("Broadcast on the Channel {1} items. Time Elapsed: {0}", secondElapsedToBroadcast, upper));

            List<BinaryTestModel> elem = b.UnLoadCargo<List<BinaryTestModel>>().First();

            Assert.AreEqual(elem.Count(), 1000, "Not every elements have been broadcasted");
            Assert.IsTrue(secondElapsedToAdd < 5000, "Add took more than 5 second. Review the logic, performance must be 10000 elems in less than 5 sec");
            Assert.IsTrue(secondElapsedToBroadcast < 3000, "Broadcast took more than 3 second. Review the logic, performance must be 10000 elems in less than 5 sec");
        }
        
        [TestMethod]
        public void BsonStressTest()
        {
            Truck b = new Truck("MAN", Shared.Options.SerializationOption.JSON);

            double upper = Math.Pow(10, 3);

            Stopwatch sw = new Stopwatch();

            sw.Start();
             
            b.LoadCargo(BsonTestModels.ToList());
            
            sw.Stop();

            long secondElapsedToAdd = sw.ElapsedMilliseconds ;

            Trace.WriteLine(string.Format("Put on the Channel {1} items. Time Elapsed: {0}", secondElapsedToAdd, upper));
            sw.Reset();
            sw.Start();

            b.DeliverTo("Mom");
            sw.Stop();

            long secondElapsedToBroadcast = sw.ElapsedMilliseconds;

            Trace.WriteLine(string.Format("Broadcast on the Channel {1} items. Time Elapsed: {0}", secondElapsedToBroadcast, upper));
            b.WaitDelivery("Mom").Wait();

            var elem = b.UnLoadCargo<List<BsonTestModel>>().First();

            Assert.AreEqual(elem.Count(), 1000, "Not every elements have been broadcasted");
            Assert.IsTrue(secondElapsedToAdd < 5000, "Add took more than 5 second. Review the logic, performance must be 10000 elems in less than 5 sec");
            Assert.IsTrue(secondElapsedToBroadcast < 3000, "Broadcast took more than 3 second. Review the logic, performance must be 10000 elems in less than 5 sec");
        }
    }
}