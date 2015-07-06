using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Sprache;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBvhParser()
        {
            var path = "../../../samples/simple.bvh";
            var text = File.ReadAllText(path);

            var root = MMIO.Bvh.BvhParse.Parser.Parse(text);

            Assert.AreEqual("Hips", root.Name);
        }

        [TestMethod]
        public void TestPmdParser()
        {
            var path = "../../../samples/初音ミクVer2.pmd";
            var bytes = File.ReadAllBytes(path);

            var model = MMIO.Mmd.PmdParse.Parse(new ArraySegment<byte>(bytes)).Value;

            Assert.AreEqual("初音ミク", model.Header.Name);
            Assert.AreEqual(12354, model.Vertices.Length);
            Assert.AreEqual(22961 * 3, model.Indices.Length);
            Assert.AreEqual(17, model.Materials.Length);
        }
    }
}
