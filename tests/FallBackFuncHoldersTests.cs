﻿using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallBackFuncHoldersTests
	{
		[Test]
		public void Should_BoxingSafeConverter_Work_For_Structs()
		{
			var res = BoxingSafeConverter<int, int>.Instance.Convert(5);
			var res2 = BoxingSafeConverter<double, double>.Instance.Convert(5d);
			Assert.AreEqual(typeof(int), res.GetType());
			Assert.AreEqual(typeof(double), res2.GetType());
		}

		[Test]
		public void Should_BoxingSafeConverter_Work_For_Classes()
		{
			var cls = new TestClass();
			var res = BoxingSafeConverter<TestClass, TestClass>.Instance.Convert(cls);
			Assert.AreEqual(typeof(TestClass), res.GetType());
		}

		[Test]
		public void Should_FallBackFuncHolder_Work()
		{
			int i = 1;
			var holder = new FallBackFuncHolder<int>((_) => ++i);
			var theSameFunc = holder.GetFallbackFunc<int>();
			var res = theSameFunc(new CancellationToken());
			Assert.AreEqual(2, res);
		}

		[Test]
		public void Should_FallBackFuncHolder_BeNull_If_Types_Not_Equals()
		{
			int i = 1;
			var holder = new FallBackFuncHolder<int>((_) => ++i);
			var theSameFunc = holder.GetFallbackFunc<string>();
			Assert.IsNull(theSameFunc);
		}

		[Test]
		public async Task Should_FallBackAsyncFuncHolder_Work()
		{
			int i = 1;
			var holder = new FallBackAsyncFuncHolder<int>(async (_) => await Task.FromResult(++i));
			var theSameFunc = holder.GetFallbackAsyncFunc<int>();
			var res = await theSameFunc(new CancellationToken());
			Assert.AreEqual(2, res);
		}

		[Test]
		public void Should_FallBackAsyncFuncHolder_BeNull_If_Types_Not_Equals()
		{
			int i = 1;
			var holder = new FallBackAsyncFuncHolder<int>(async (_) => await Task.FromResult(++i));
			var theSameFunc = holder.GetFallbackAsyncFunc<string>();
			Assert.IsNull(theSameFunc);
		}

#pragma warning disable S2094 // Classes should not be empty
		private class TestClass { }
#pragma warning restore S2094 // Classes should not be empty

	}
}