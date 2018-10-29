using System;
using System.Collections.Generic;
using System.Linq;
using BridgeVs.DynamicVisualizers.Template;
using BridgeVs.Shared.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BridgeVs.DynamicVisualizers.Test.Template
{
    [TestClass]
    public class MessageTest
    {

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Message_of_anonymous_list_should_return_AnonymousType()
        {
            var a = new { str1 = Guid.NewGuid().ToString(), str2 = Guid.NewGuid().ToString() };

            var res = (from i in Enumerable.Range(0, 2)
                select a).ToList();

            Message message = new Message(Guid.NewGuid().ToString(), SerializationOption.BinarySerializer, res.GetType());

            Assert.IsTrue(message.AssemblyName.Equals("mscorlib"));
            Assert.IsTrue(message.FileName.Equals("List(AnonymousType(String, String))"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Message_of_list_of_int_should_return_List_int()
        {
            List<int> res = (from i in Enumerable.Range(0, 2)
                select i).ToList();

            Message message = new Message(Guid.NewGuid().ToString(), SerializationOption.BinarySerializer, res.GetType());

            Assert.IsTrue(message.AssemblyName.Equals("mscorlib"));
            Assert.AreEqual(message.FileName, "List(Int32)", "List of int should return List(Int32)");
        }



        [TestMethod]
        [TestCategory("UnitTest")]
        public void CalculateFileNameFromTypeByCallingBridgeVsDynamicVisualizersTemplateMessage_ctor_ReturnsToStringIsFileNameObject_truck()
        {
            // act
            Message result = new Message("truckId", SerializationOption.XmlSerializer, typeof(object));

            // assert
            Assert.AreEqual("FileName: Object\r\nTypeFullName: System.Object\r\nTypeName: Object\r\nTyp" +
                            "eNamespace: System\r\nTruckId: truckId\r\nSerializationType: XmlSerializer", result.ToString());
        }


    }
}
