using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using LINQBridgeVs.DynamicCore;
using LINQBridgeVs.DynamicCore.Template;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicCore.UnitTest
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
            var r = TestQuery.SkipWhile(i => i < 45);

            var message = DeserializeMessage(r);


            Assert.AreEqual(message.FileName, "IEnumerable(Int32).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.IEnumerable<System.Int32>");
            Assert.AreEqual(message.TypeName, "IEnumerableInt32");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }

       

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_OfTypeIteratorShouldSucceed()
        {
            var r = TestQuery.OfType<short>();

            var message = DeserializeMessage(r);

            Assert.AreEqual(message.FileName, "IEnumerable(Int16).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.IEnumerable<System.Int16>");
            Assert.AreEqual(message.TypeName, "IEnumerableInt16");
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

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BroadCastData_AnonymousShouldSucceed()
        {
            var r = TestQuery.Select(i => new { Value = i });

            var message = DeserializeMessage(r);

            Assert.AreEqual(message.FileName, "IEnumerable(AnonymousType(Int32)).linq");
            Assert.AreEqual(message.TypeFullName, "System.Collections.Generic.IEnumerable<AnonymousType<System.Int32>>");
            Assert.AreEqual(message.TypeName, "IEnumerableAnonymousTypeInt32");
            Assert.AreEqual(message.TypeNamespace, "System.Collections.Generic");
        }

        private static Message DeserializeMessage(object @object)
        {
            using (var memoryStream = new MemoryStream())
            {
                DynamicObjectSource.BroadCastData(@object, memoryStream);
                var binaryFormatter = new BinaryFormatter();
                memoryStream.Position = 0;
                return (Message)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
