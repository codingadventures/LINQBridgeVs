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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Model.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.DynamicVisualizers.Test
{
    [TestClass]
    public class DynamicObjectSourceTest
    {
        private static readonly IEnumerable<int> TestQuery
            = from i in Enumerable.Range(1, 100)
              where i > 23
              select i;

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_SkipWhileIteratorShouldSucceed()
        {
            var r = TestQuery.SkipWhile(i => i < 45).ToList();

            var message = DeserializeMessage(r);


            Assert.AreEqual(message.FileName, "List(Int32).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.List<System.Int32>");
            Assert.AreEqual(message.TypeName, "ListInt32");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_OfTypeIteratorShouldSucceed()
        {
            var r = TestQuery.OfType<short>().ToList(); ;

            var message = DeserializeMessage(r);

            Assert.AreEqual(message.FileName, "List(Int16).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.List<System.Int16>");
            Assert.AreEqual(message.TypeName, "ListInt16");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_DictionaryShouldSucceed()
        {
            var r = new Dictionary<int, object> { { 1, "Test" } };

            var message = DeserializeMessage(r);

            Assert.AreEqual(message.FileName, "Dictionary(Int32, Object).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.Dictionary<System.Int32, System.Object>");
            Assert.AreEqual(message.TypeName, "DictionaryInt32Object");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }
        //for now
        //[TestMethod]
        //[TestCategory("UnitTest")]
        //public void BroadCastData_AnonymousShouldSucceed()
        //{
        //    var r = TestQuery.Select(i => new { Value = i });

        //    var message = DeserializeMessage(r);

        //    Assert.AreEqual(message.FileName, "IEnumerable(AnonymousType(Int32)).linq");
        //    Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.IEnumerable<AnonymousType<System.Int32>>");
        //    Assert.AreEqual(message.TypeName, "IEnumerableAnonymousTypeInt32");
        //    Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        //}

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_CustomObjectShouldSucceed()
        {
            IEnumerable<CustomType1> r = TestQuery.Select(i => new CustomType1());

            Message message = DeserializeMessage(r);

            Assert.AreEqual(message.FileName, "IEnumerable(BridgeVs.Model.Test.CustomType1).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.IEnumerable<BridgeVs.Model.Test.CustomType1>");
            Assert.AreEqual(message.TypeName, "IEnumerableCustomType1");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }

        private static Message DeserializeMessage(object @object)
        {
            using (var memoryStream = new MemoryStream())
            {
                DynamicObjectSource dynamicObjectSource = new DynamicObjectSource();
                dynamicObjectSource.BroadCastData(@object, memoryStream);
                var binaryFormatter = new BinaryFormatter();
                memoryStream.Position = 0;
                return (Message)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}