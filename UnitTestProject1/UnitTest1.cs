﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;


namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        static string SAMPLE_DIRECTORY = "../../../samples/";

        [TestMethod]
        public void TestBvhParser()
        {
            var path = SAMPLE_DIRECTORY+"simple.bvh";
            var text = File.ReadAllText(path, Encoding.GetEncoding(932));

            var bvh = MMIO.Bvh.BvhParse.Execute(text, false);

            Assert.AreEqual("Hips", bvh.Root.Name);
            Assert.AreEqual(2, bvh.MotionProperties.Count);
            Assert.AreEqual(1, bvh.Frames.Count());
            Assert.AreEqual(12, bvh.Frames.First().Count());
        }

        [TestMethod]
        public void TestVpdParser()
        {
            var path = SAMPLE_DIRECTORY + "右手グー.vpd";
            var text = File.ReadAllText(path, Encoding.GetEncoding(932));

            var pose = MMIO.Mmd.VpdParse.Execute(text);

            Assert.AreEqual(14, pose.Bones.Length);
        }

        [TestMethod]
        public void TestPmdParser()
        {
            var path = SAMPLE_DIRECTORY+"初音ミクVer2.pmd";
            var bytes = File.ReadAllBytes(path);

            var model = MMIO.Mmd.PmdParse.Execute(bytes);

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
            var path = SAMPLE_DIRECTORY+"初音ミクVer2.pmx";
            var bytes = File.ReadAllBytes(path);

            var model = MMIO.Mmd.PmxParse.Execute(bytes);
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

        [TestMethod]
        public void TestVmdParser()
        {
            var path = SAMPLE_DIRECTORY + "sample.vmd";
            var bytes = File.ReadAllBytes(path);

            var vmd = MMIO.Mmd.VmdParse.Execute(bytes);
        }
    }
}
