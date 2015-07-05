﻿using System;
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

            var root = MMIO.Bvh.BvhParser.Parser.Parse(text);

            Assert.AreEqual("Hips", root.Name);
        }

        [TestMethod]
        public void TestPmdParser()
        {
            var path = "../../../samples/miku_v2.pmd";
            var bytes = File.ReadAllBytes(path);

            var model = MMIO.Mmd.PmdParser.Parser(new ArraySegment<byte>(bytes)).Value;

            Assert.AreEqual("初音ミク", model.Name);
        }
    }
}