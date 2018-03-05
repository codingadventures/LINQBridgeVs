#region License
// Copyright (c) 2013 - 2018 Giovanni Campo
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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using BridgeVs.SInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SInject.Test
{
    [TestClass]
    public class SerializationTest
    {
        private static Assembly _modelAssembly;

        private static string SinjectTestModelPath
        {
            get
            {
                var testModelDll = Path.Combine("Temp", "SInject.Test.Model.dll");
                var currentLocation = Path.GetDirectoryName((typeof(SerializationTest).Assembly.Location));
                if (currentLocation == null) return string.Empty;
                var parentDirectory = Directory.GetParent(currentLocation).FullName;
#if NET35 || NET20
               
                var path = Path.Combine(Directory.GetParent(parentDirectory).FullName, testModelDll);
#else
                var path = Path.Combine(parentDirectory, testModelDll);
#endif


                return path;
            }
        }
        /// <summary>
        /// Initializes the specified test context. Patch with SInject the Model Library
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var sInjection = new SInjection(SinjectTestModelPath);
            sInjection.Patch(SerializationTypes.BinarySerialization);
            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
            _modelAssembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(SinjectTestModelPath));


        }
        static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return args.Name.Contains("SInject.Test.Model") ? AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(SinjectTestModelPath)) : null;
        }

        private object _notSerializableInerhitsDictionary;
        private object _notSerializableObject;

        [TestInitialize]
        public void Init()
        {
            //Load the types
            _notSerializableInerhitsDictionary = _modelAssembly.CreateInstance("SInject.Test.Model.NotSerializableInerhitsDictionary");
            _notSerializableObject = _modelAssembly.CreateInstance("SInject.Test.Model.NotSerializableObject");
        }

        [TestMethod]
        public void Not_Serializable_Inerhits_Dictionary_Should_Serialize_After_Patching()
        {
            var f = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                f.Serialize(stream, _notSerializableInerhitsDictionary);
                stream.Position = 0;
                var obj = f.Deserialize(stream);

                Assert.IsNotNull(obj, "Object hasn't been serialized");
                Assert.IsTrue(obj.GetType() == _notSerializableInerhitsDictionary.GetType(), "Object is not of the same type");
            }
        }

        [TestMethod]
        public void Not_Serializable_Object_Should_Serialize_After_Patching()
        {
            var f = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                f.Serialize(stream, _notSerializableObject);
                stream.Position = 0;
                var obj = f.Deserialize(stream);
                Assert.IsNotNull(obj, "Object hasn't been serialized");
                Assert.IsTrue(obj.GetType() == _notSerializableObject.GetType(), "Object is not of the same type");
            }
        }

    }
}
