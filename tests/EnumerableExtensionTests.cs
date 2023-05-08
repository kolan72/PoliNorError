using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static PoliNorError.EnumerableExtension;

namespace PoliNorError.Tests
{
	internal class EnumerableExtensionTests
    {
        [Test]
        public void Should_SkipLast_Work_ForEmpty()
        {
            var res = Array.Empty<int>().SkipLast();
            Assert.AreEqual(0, res.Count());
        }

        [Test]
        public void Should_SkipLast_Work_ForOneElement()
        {
            var res = new List<int>() { 1 };
            Assert.AreEqual(0, res.SkipLast().Count());
        }

        [Test]
        public void Should_SkipLast_Work_ForManyElements()
        {
            var res = new List<int>() { 1, 2, 3 };
            Assert.AreEqual(2, res.SkipLast().Count());
            Assert.AreEqual(1, res.FirstOrDefault());
            Assert.AreEqual(2, res.Skip(1).FirstOrDefault());
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne1()
        {
            var res = new List<int>() { 1, 0, 0};
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            Assert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            Assert.AreEqual(0, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne2()
        {
            var res = new List<int>() { 0, 1, 0 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            Assert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            Assert.AreEqual(1, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne3()
        {
            var res = new List<int>() { 0, 0, 1 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            Assert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            Assert.AreEqual(2, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForNoOne()
        {
            var res = new List<int>() { 0, 0, 0 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            Assert.AreEqual(ConditionCheckResult.ConditionMetType.NoOne, chr1.ConditionMet);
        }
    }
}