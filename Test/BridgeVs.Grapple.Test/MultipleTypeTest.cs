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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.Grapple.Test
{
    [TestClass]
    public class MultipleTypeTest
    {


        [TestInitialize]
        public void Init()
        {
            Truck bus = new Truck("GlobeTrotter");

            List<int> intList = new List<int> { 123, 12, 312, 312, 3 };
            bus.LoadCargo(intList);

            List<string> stringList = new List<string> { "asdasd", "asdasd123123" };
            bus.LoadCargo(stringList);

            LinkedList<string> linkedList = new LinkedList<string>();
            linkedList.AddFirst(new LinkedListNode<string>("Hello"));
            linkedList.AddLast(new LinkedListNode<string>("World!"));
            bus.LoadCargo(linkedList);
            bus.DeliverTo("Mom");

        }


        [TestMethod]
        public void Verify_List_String_Is_Not_Null_And_Has_2_Elements()
        {
            

            Truck bus = new Truck("GlobeTrotter");
            bus.WaitDelivery("Mom").Wait() ;

            List<string> ret = bus.UnLoadCargo<List<string>>().First();
            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 2);
        }

        [TestMethod]
        public void Verify_List_Int_Is_Not_Null_And_Has_5_Elements()
        {
            Truck bus = new Truck("GlobeTrotter");
            bus.WaitDelivery("Mom").Wait();

            List<int> ret = bus.UnLoadCargo<List<int>>().First();
            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 5);



        }
        [TestMethod]
        public void Verify_LinekdList_String_Is_Not_Null_And_Has_2_Elements()
        {
            Truck bus = new Truck("GlobeTrotter");
            bus.WaitDelivery("Mom").Wait();
            LinkedList<string> ret = bus.UnLoadCargo<LinkedList<string>>().First();
            Assert.IsNotNull(ret);
            Assert.IsTrue(ret.Count == 2);
        }
    }
}
