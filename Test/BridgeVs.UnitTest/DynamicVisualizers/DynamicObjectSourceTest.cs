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

using BridgeVs.DynamicVisualizers;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace BridgeVs.UnitTest.DynamicVisualizers
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
            List<int> r = TestQuery.SkipWhile(i => i < 45).ToList();

            Message message = DeserializeMessage(r);
            Assert.AreEqual("List(Int32)", message.FileName);
            Assert.AreEqual("System.Collections.Generic.List<System.Int32>", message.TypeFullName);
            Assert.AreEqual("ListInt32", message.TypeName);
            Assert.AreEqual("System.Collections.Generic", message.TypeNamespace);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_OfTypeIteratorShouldSucceed()
        {
            List<short> r = TestQuery.OfType<short>().ToList(); ;

            Message message = DeserializeMessage(r);

            Assert.AreEqual("List(Int16)", message.FileName);
            Assert.AreEqual("System.Collections.Generic.List<System.Int16>", message.TypeFullName);
            Assert.AreEqual("ListInt16", message.TypeName);
            Assert.AreEqual("System.Collections.Generic", message.TypeNamespace);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_DictionaryShouldSucceed()
        {
            Dictionary<int, object> r = new Dictionary<int, object> { { 1, "Test" } };

            Message message = DeserializeMessage(r);

            Assert.AreEqual("Dictionary(Int32, Object)", message.FileName);
            Assert.AreEqual("System.Collections.Generic.Dictionary<System.Int32, System.Object>", message.TypeFullName);
            Assert.AreEqual("DictionaryInt32Object", message.TypeName);
            Assert.AreEqual("System.Collections.Generic", message.TypeNamespace);
        }
        //for now
        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_AnonymousShouldSucceed()
        {
            var r = TestQuery.Select(i => new { Value = i });

            Message message = DeserializeMessage(r);

            Assert.AreEqual("IEnumerable(AnonymousType(Int32))", message.FileName);
            Assert.AreEqual("System.Collections.Generic.IEnumerable<AnonymousType<System.Int32>>", message.TypeFullName);
            Assert.AreEqual("IEnumerableAnonymousTypeInt32", message.TypeName);
            Assert.AreEqual("System.Collections.Generic", message.TypeNamespace);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_CustomObjectShouldSucceed()
        {
            IEnumerable<CustomType1> r = TestQuery.Select(i => new CustomType1());

            Message message = DeserializeMessage(r);

            Assert.AreEqual("IEnumerable(CustomType1)", message.FileName);
            Assert.AreEqual($"System.Collections.Generic.IEnumerable<{typeof(CustomType1).Assembly.GetName().Name}.CustomType1>", message.TypeFullName);
            Assert.AreEqual("IEnumerableCustomType1", message.TypeName);
            Assert.AreEqual("System.Collections.Generic", message.TypeNamespace);
        }

        private static Message DeserializeMessage(object @object)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DynamicObjectSource dynamicObjectSource = new DynamicObjectSource();
                dynamicObjectSource.GetData(@object, memoryStream);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                memoryStream.Position = 0;
                return (Message)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}