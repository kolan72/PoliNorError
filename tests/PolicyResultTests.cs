using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class PolicyResultTests
    {
        [Test]
        public void Should_FlexSyncEnumerable_Work_For_Sync_Add()
        {
			var collection = new FlexSyncEnumerable<Exception>
			{
				new Exception()
			};
			ClassicAssert.AreEqual(1, collection.Count());
        }

        [Test]
        public void Should_FlexSyncEnumerable_Work_For_Async_Add()
        {
			var collection = new FlexSyncEnumerable<Exception>(true)
			{
				new Exception()
			};
			ClassicAssert.AreEqual(1, collection.Count());
        }

        [Test]
        public void Should_FlexSyncEnumerable_Work_For_Sync_AddRange()
        {
            var collection = new FlexSyncEnumerable<Exception>();
            collection.AddRange(new List<Exception>() { new Exception(), new Exception() });
            ClassicAssert.AreEqual(2, collection.Count());
        }

        [Test]
        public void Should_FlexSyncEnumerable_Work_For_Async_AddRange()
        {
            var collection = new FlexSyncEnumerable<Exception>(true);
            collection.AddRange(new List<Exception>() { new Exception(), new Exception() });
            ClassicAssert.AreEqual(2, collection.Count());
        }
    }
}