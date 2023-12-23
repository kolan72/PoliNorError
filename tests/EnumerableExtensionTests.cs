using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual(0, res.Count());
        }

        [Test]
        public void Should_SkipLast_Work_ForOneElement()
        {
            var res = new List<int>() { 1 };
            ClassicAssert.AreEqual(0, res.SkipLast().Count());
        }

        [Test]
        public void Should_SkipLast_Work_ForManyElements()
        {
            var res = new List<int>() { 1, 2, 3 };
            ClassicAssert.AreEqual(2, res.SkipLast().Count());
            ClassicAssert.AreEqual(1, res.FirstOrDefault());
            ClassicAssert.AreEqual(2, res.Skip(1).FirstOrDefault());
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne1()
        {
            var res = new List<int>() { 1, 0, 0};
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            ClassicAssert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            ClassicAssert.AreEqual(0, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne2()
        {
            var res = new List<int>() { 0, 1, 0 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            ClassicAssert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            ClassicAssert.AreEqual(1, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForOnlyOne3()
        {
            var res = new List<int>() { 0, 0, 1 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            ClassicAssert.AreEqual(ConditionCheckResult.ConditionMetType.OnlyOne, chr1.ConditionMet);
            ClassicAssert.AreEqual(2, chr1.Indx);
        }

        [Test]
        public void Should_CheckCollectionsForCondition_Work_ForNoOne()
        {
            var res = new List<int>() { 0, 0, 0 };
			bool f(int i) => i == 1;
			var chr1 = res.CheckCollectionsForCondition(f);
            ClassicAssert.AreEqual(ConditionCheckResult.ConditionMetType.NoOne, chr1.ConditionMet);
        }
    }
}