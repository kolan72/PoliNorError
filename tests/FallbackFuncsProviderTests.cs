﻿using NUnit.Framework;
using System;
using System.IO;
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
			Action<CancellationToken> resultAction;
			var i = 0;
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
				case TestFallbackFuncType.FromNonGeneric:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testFallbackFuncType), testFallbackFuncType, null);
			}
		}

		[Test]
		[TestCase(TestFallbackFuncType.NoFuncs)]
		[TestCase(TestFallbackFuncType.Exists)]
		[TestCase(TestFallbackFuncType.CrossSync)]
		public async Task Should_GetAsyncFallbackFunc_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider(false);
			Func<CancellationToken, Task> resultFunc;
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
			var provider = new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate ?? false);
			Func<CancellationToken, int> resultFunc;
			var i = 0;
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
					if (crossSync == true)
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
				case TestFallbackFuncType.CrossSync:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testFallbackFuncType), testFallbackFuncType, null);
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
			var provider = new FallbackFuncsProvider(onlyGenericFallbackForGenericDelegate ?? false);
			Func<CancellationToken, Task<int>> resultFunc;
			var i = 0;
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
					if (crossSync == true)
					{
						provider.Fallback = (_) => i++;
					}
					else
					{
						provider.FallbackAsync = async (_) => { await Task.Delay(1); i++; };
					}
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					await resultFunc(default);
					Assert.That(i, onlyGenericFallbackForGenericDelegate == false ? Is.EqualTo(1) : Is.EqualTo(0));
					break;
				case TestFallbackFuncType.CrossSync:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testFallbackFuncType), testFallbackFuncType, null);
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
					switch (forAllNonGeneric)
					{
						case true:
							funcsProvider = FallbackFuncsProvider.Create(async (_) => await Task.Delay(1), (_) => { });
							break;
						case false:
							funcsProvider = FallbackFuncsProvider.Create();
							break;
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

		[Test]
		public void Should_SetFallbackAction_Work()
		{
			void act1(CancellationToken _)
			{
				// Method intentionally left empty.
			}
			void act2(CancellationToken _)
			{
				// Method intentionally left empty.
			}

			var testProvider = new TestFallbackFuncsProvider();
			testProvider.SetAction(act1);
			Assert.That(testProvider.GetFallbackAction(), Is.EqualTo((Action<CancellationToken>)act1));
			testProvider.SetAction(act2);
			Assert.That(testProvider.GetFallbackAction(), Is.EqualTo((Action<CancellationToken>)act2));
		}

		[Test]
		[TestCase(CancellationType.Cancelable)]
		[TestCase(CancellationType.Precancelable)]
		public void Should_SetFallbackAction_WithCancellationType_Work(CancellationType cancellationType)
		{
			void act1()
			{
				// Method intentionally left empty.
			}
			void act2()
			{
				// Method intentionally left empty.
			}

			var testProvider = new TestFallbackFuncsProvider();
			testProvider.SetAction(act1, cancellationType);
			var funcThatWasSet = testProvider.GetFallbackAction();
			Assert.That(funcThatWasSet, Is.Not.Null);

			testProvider.SetAction(act2, cancellationType);
			var funcThatWasSet2 = testProvider.GetFallbackAction();
			Assert.That(funcThatWasSet2, Is.Not.Null);
			Assert.That(funcThatWasSet2, Is.Not.EqualTo(funcThatWasSet));
		}

		[Test]
		public void Should_SetAsyncFunc_Work()
		{
			Task fn1(CancellationToken _)
			{
				return Task.CompletedTask;
			}

			Task fn2(CancellationToken _)
			{
				return Task.CompletedTask;
			}

			var testProvider = new TestFallbackFuncsProvider();
			testProvider.SetAsyncFunc(fn1);
			Assert.That(testProvider.GetAsyncFallbackFunc(), Is.EqualTo((Func<CancellationToken, Task>)fn1));
			testProvider.SetAsyncFunc(fn2);
			Assert.That(testProvider.GetAsyncFallbackFunc(), Is.EqualTo((Func<CancellationToken, Task>)fn2));
		}

		[Test]
		[TestCase(CancellationType.Cancelable)]
		[TestCase(CancellationType.Precancelable)]
		public void Should_SetAsyncFunc_WithCancellationType_Work(CancellationType cancellationType)
		{
			Task fn1() => Task.CompletedTask;
			Task fn2() => Task.CompletedTask;

			var testProvider = new TestFallbackFuncsProvider();
			testProvider.SetAsyncFunc(fn1, cancellationType);
			var funcThatWasSet = testProvider.GetAsyncFallbackFunc();
			Assert.That(funcThatWasSet, Is.Not.Null);

			testProvider.SetAsyncFunc(fn2, cancellationType);
			var funcThatWasSet2 = testProvider.GetAsyncFallbackFunc();
			Assert.That(funcThatWasSet2, Is.Not.Null);
			Assert.That(funcThatWasSet2, Is.Not.EqualTo(funcThatWasSet));
		}

		[Test]
		public void Should_CreateFallbackPolicy_When_ToFallbackPolicyIsCalled()
		{
			const string FNF = "File not found";
			var fb = FallbackFuncsProvider.Create()
							.AddOrReplaceFallbackFunc((_) => FNF)
							.AddOrReplaceFallbackFunc((_) => Array.Empty<string>())
							.ToFallbackPolicy()
							.IncludeError<FileNotFoundException>();

			var resAllText = fb.Handle((fn) => File.ReadAllText(fn), "f.txt");
			Assert.That(resAllText.IsPolicySuccess, Is.True);
			Assert.That(resAllText.Result, Is.EqualTo(FNF));

			var resAllLines = fb.Handle((fn) => File.ReadAllLines(fn), "f.txt");
			Assert.That(resAllLines.IsPolicySuccess, Is.True);
			Assert.That(resAllLines.Result.IsEmpty, Is.True);
		}

		internal enum TestFallbackFuncType
		{
			NoFuncs,
			Exists,
			CrossSync,
			FromNonGeneric
		}

		private class TestFallbackFuncsProvider : FallbackFuncsProvider
		{
			public TestFallbackFuncsProvider() : base(false){}

			public void SetAction(Action<CancellationToken> action)
			{
				SetFallbackAction(action);
			}

			public void SetAction(Action action, CancellationType convertType = CancellationType.Precancelable)
			{
				SetFallbackAction(action, convertType);
			}

			public void SetAsyncFunc(Func<CancellationToken, Task> func)
			{
				SetAsyncFallbackFunc(func);
			}

			public void SetAsyncFunc(Func<Task> func, CancellationType convertType = CancellationType.Precancelable)
			{
				SetAsyncFallbackFunc(func, convertType);
			}
		}
	}
}
