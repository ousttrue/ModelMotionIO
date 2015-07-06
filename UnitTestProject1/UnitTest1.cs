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

            var model = MMIO.Mmd.PmdParse.Parse(bytes);

            Assert.AreEqual("初音ミク", model.Header.Name);
            Assert.AreEqual(12354, model.Vertices.Length);
            Assert.AreEqual(22961 * 3, model.Indices.Length);
            Assert.AreEqual(17, model.Materials.Length);
            Assert.AreEqual(140, model.Bones.Length);
            Assert.AreEqual(7, model.IKList.Length);
            Assert.AreEqual(31, model.Morphs.Length);
            Assert.AreEqual(45, model.Rigidbodies.Length);
            Assert.AreEqual(27, model.Joints.Length);
        }

        [TestMethod]
        public void TestPmxParser()
        {
            var path = "../../../samples/初音ミクVer2.pmx";
            var bytes = File.ReadAllBytes(path);

            var model = MMIO.Mmd.PmxParse.Parse(bytes);
            Assert.AreEqual("初音ミク", model.Header.Name);
            Assert.AreEqual(12354, model.Vertices.Length);
            Assert.AreEqual(22961 * 3, model.Indices.Length);
            Assert.AreEqual(17, model.Materials.Length);
            Assert.AreEqual(140, model.Bones.Length);
            Assert.AreEqual(7, model.Bones.Where(x => x.BoneFlag.HasFlag(MMIO.Mmd.PmxBoneFlags.IKEffector)).Count());
            Assert.AreEqual(30, model.Morphs.Length);
            Assert.AreEqual(45, model.Rigidbodies.Length);
            Assert.AreEqual(27, model.Joints.Length);
        }
    }
}
