﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackFuncsProviderTests
	{
		[Test]
		[TestCase(TestFallbackFuncType.NoFuncs)]
		[TestCase(TestFallbackFuncType.Exists)]
		[TestCase(TestFallbackFuncType.CrossSync)]
		public void Should_GetFallbackAction_Return_Preset_Action(TestFallbackFuncType testFallbackFuncType)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider(false);
			Action<CancellationToken> resultAction = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultAction = provider.GetFallbackAction();
					Assert.That(resultAction, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.Fallback = (_) => i++;
					resultAction =  provider.GetFallbackAction();
					resultAction(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.CrossSync:
					provider.FallbackAsync = async(_) => { await Task.Delay(1); i++; };
					resultAction = provider.GetFallbackAction();
					resultAction(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		[Test]
		[TestCase(TestFallbackFuncType.NoFuncs)]
		[TestCase(TestFallbackFuncType.Exists)]
		[TestCase(TestFallbackFuncType.CrossSync)]
		public async Task Should_GetAsyncFallbackFunc_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider(false);
			Func<CancellationToken, Task> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetAsyncFallbackFunc();
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.FallbackAsync = async(_) => { await Task.Delay(1); i++; };
					resultFunc = provider.GetAsyncFallbackFunc();
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.CrossSync:
					provider.Fallback = (_) => i++;
					resultFunc = provider.GetAsyncFallbackFunc();
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		[TestCase(TestFallbackFuncType.NoFuncs, null, null)]
		[TestCase(TestFallbackFuncType.Exists, null, null)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true, false)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false, false)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true, true)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false, true)]
		public void Should_GetFallbackFuncT_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType, bool? crossSync, bool? onlyGenericFallbackForGenericDelegate)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate ?? false);
			Func<CancellationToken, int> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetFallbackFunc<int>();
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.SetFallbackFunc((_) => i++);
					resultFunc = provider.GetFallbackFunc<int>();
					resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.FromNonGeneric:
					if (crossSync.Value)
					{
						provider.FallbackAsync = async (_) => { await Task.Delay(1); i++; };
					}
					else
					{
						provider.Fallback = (_) => i++;
					}
					resultFunc = provider.GetFallbackFunc<int>();
					resultFunc(default);
					if (onlyGenericFallbackForGenericDelegate == false)
					{
						Assert.That(i, Is.EqualTo(1));
					}
					else if(onlyGenericFallbackForGenericDelegate == true)
					{
						Assert.That(i, Is.EqualTo(0));
					}
					break;
			}
		}

		[TestCase(TestFallbackFuncType.NoFuncs, null, null)]
		[TestCase(TestFallbackFuncType.Exists, null, null)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true, false)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false, false)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true, true)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false, true)]
		public async Task Should_GetAsyncFallbackFuncT_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType, bool? crossSync, bool? onlyGenericFallbackForGenericDelegate)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate ?? false);
			Func<CancellationToken, Task<int>> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.SetAsyncFallbackFunc(async (_) => { await Task.Delay(1); i++; return 0; });
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.FromNonGeneric:
					if (crossSync.Value)
					{
						provider.Fallback = (_) => i++;
					}
					else
					{
						provider.FallbackAsync = async (_) => { await Task.Delay(1); i++; };
					}
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					await resultFunc(default);
					if (onlyGenericFallbackForGenericDelegate == false)
					{
						Assert.That(i, Is.EqualTo(1));
					}
					else
					{
						Assert.That(i, Is.EqualTo(0));
					}
					break;
			}
		}

		[Test]
		[TestCase(FallbackTypeForTests.WithAction, null)]
		[TestCase(FallbackTypeForTests.WithAsyncFunc, null)]
		[TestCase(null, true)]
		[TestCase(null, false)]
		public void Should_Create_Return_Correct_Instance(FallbackTypeForTests? fallbackTypeForTests, bool? forAllNonGeneric)
		{
			FallbackFuncsProvider funcsProvider = null;
			switch (fallbackTypeForTests)
			{
				case FallbackTypeForTests.WithAction:
					funcsProvider = FallbackFuncsProvider.Create((_) => { });
					Assert.That(funcsProvider.HasFallbackAction(), Is.True);
					break;
				case FallbackTypeForTests.WithAsyncFunc:
					funcsProvider = FallbackFuncsProvider.Create(async (_) => await Task.Delay(1));
					Assert.That(funcsProvider.HasAsyncFallbackFunc(), Is.True);
					break;
				case null:
					if (forAllNonGeneric == true)
					{
						funcsProvider = FallbackFuncsProvider.Create(async (_) => await Task.Delay(1), (_) => { });
					}
					else if (forAllNonGeneric == false)
					{
						funcsProvider = FallbackFuncsProvider.Create();
					}
					Assert.That(funcsProvider.HasFallbackAction(), Is.EqualTo(forAllNonGeneric));
					Assert.That(funcsProvider.HasAsyncFallbackFunc(), Is.EqualTo(forAllNonGeneric));
					break;
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddOrReplaceFallbackFunc_Work(bool funcWithToken)
		{
			var provider = FallbackFuncsProvider.Create();
			if (funcWithToken)
			{
				Assert.That(provider.AddOrReplaceFallbackFunc((_) => 1).HasFallbackFunc<int>(), Is.True);
			}
			else
			{
				Assert.That(provider.AddOrReplaceFallbackFunc(() => 1).HasFallbackFunc<int>(), Is.True);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddOrReplaceAsyncFallbackFunc_Work(bool funcWithToken)
		{
			var provider = FallbackFuncsProvider.Create();
			if (funcWithToken)
			{
				Assert.That(provider.AddOrReplaceAsyncFallbackFunc(async (_) => {await Task.Delay(1); return 1;}).HasAsyncFallbackFunc<int>(), Is.True);
			}
			else
			{
				Assert.That(provider.AddOrReplaceAsyncFallbackFunc(async () => {await Task.Delay(1); return 1;}).HasAsyncFallbackFunc<int>(), Is.True);
			}
		}

		internal enum TestFallbackFuncType
		{
			NoFuncs,
			Exists,
			CrossSync,
			FromNonGeneric
		}
	}
}
