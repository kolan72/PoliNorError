using NUnit.Framework;
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
		public void Should_CreateGenericSyncFallbackBehavior_Return_Instance_With_GenericFunc(bool tokenAware)
		{
			FallbackBehavior<int> provider;
			if (tokenAware)
			{
				provider = FallbackBehavior<int>.Create((_) => 11);
			}
			else
			{
				provider = FallbackBehavior<int>.Create(() => 11);
			}

			Assert.That(provider, Is.Not.Null);
			Assert.That(provider.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Sync));
			Assert.That(provider.Fallback, Is.Not.Null);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CreateGenericAsyncFallbackBehavior_Return_Instance_With_GenericFunc(bool tokenAware)
		{
			FallbackBehavior<int> provider;
			if (tokenAware)
			{
				provider = FallbackBehavior<int>.Create(async (_) => { await Task.Delay(1); return 22; });
			}
			else
			{
				provider = FallbackBehavior<int>.Create(async () => { await Task.Delay(1); return 22; });
			}

			Assert.That(provider, Is.Not.Null);
			Assert.That(provider.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Async));
			Assert.That(provider.AsyncFallback, Is.Not.Null);
		}

		[Test]
		public void Should_CreateGenericSyncFallbackBehavior_WithCancellationType_Work()
		{
			var provider = FallbackBehavior<int>.Create(() => 33, CancellationType.Cancelable);
			Assert.That(provider.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Sync));
			Assert.That(provider.Fallback, Is.Not.Null);
		}

		[Test]
		public void Should_CreateGenericAsyncFallbackBehavior_WithCancellationType_Work()
		{
			var provider = FallbackBehavior<int>.Create(async () => { await Task.Delay(1); return 44; }, CancellationType.Cancelable);
			Assert.That(provider.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Async));
			Assert.That(provider.AsyncFallback, Is.Not.Null);
		}

		[Test]
		public void Should_FallbackBehavior_Has_ExecutionMode_None_When_Created_By_Null()
		{
			var provider1 = FallbackBehavior<int>.Create((Func<int>)null, CancellationType.Cancelable);
			var provider2 = FallbackBehavior<int>.Create((Func<CancellationToken, int>)null);
			var provider3 = FallbackBehavior<int>.Create((Func<Task<int>>)null, CancellationType.Cancelable);
			var provider4 = FallbackBehavior<int>.Create((Func<CancellationToken, Task<int>>)null);

			Assert.That(provider1.ExecutionMode, Is.EqualTo(FallbackExecutionMode.None));
			Assert.That(provider2.ExecutionMode, Is.EqualTo(FallbackExecutionMode.None));
			Assert.That(provider3.ExecutionMode, Is.EqualTo(FallbackExecutionMode.None));
			Assert.That(provider4.ExecutionMode, Is.EqualTo(FallbackExecutionMode.None));
		}

		[Test]
		public void Should_CreateFallbackBehaviorWithSyncAndAsyncDelegates_SetExecutionModeNone_WhenBothDelegatesAreNull()
		{
			var behavior = FallbackBehavior<int>.Create(null, null);

			Assert.That(behavior.ExecutionMode, Is.EqualTo(FallbackExecutionMode.None));
			Assert.That(behavior.Fallback, Is.Null);
			Assert.That(behavior.AsyncFallback, Is.Null);
		}

		[Test]
		public void Should_CreateFallbackBehaviorWithSyncAndAsyncDelegates_SetExecutionModeSync_WhenOnlySyncDelegateProvided()
		{
			Func<CancellationToken, int> fallbackFunc = _ => 7;

			var behavior = FallbackBehavior<int>.Create(fallbackFunc, null);

			Assert.That(behavior.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Sync));
			Assert.That(behavior.Fallback, Is.SameAs(fallbackFunc));
			Assert.That(behavior.AsyncFallback, Is.Null);
		}

		[Test]
		public void Should_CreateFallbackBehaviorWithSyncAndAsyncDelegates_SetExecutionModeAsync_WhenOnlyAsyncDelegateProvided()
		{
			Func<CancellationToken, Task<int>> fallbackAsync = _ => Task.FromResult(8);

			var behavior = FallbackBehavior<int>.Create(null, fallbackAsync);

			Assert.That(behavior.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Async));
			Assert.That(behavior.Fallback, Is.Null);
			Assert.That(behavior.AsyncFallback, Is.SameAs(fallbackAsync));
		}

		[Test]
		public void Should_CreateFallbackBehaviorWithSyncAndAsyncDelegates_SetExecutionModeSyncAndAsync_WhenBothDelegatesProvided()
		{
			Func<CancellationToken, int> fallbackFunc = _ => 9;
			Func<CancellationToken, Task<int>> fallbackAsync = _ => Task.FromResult(10);

			var behavior = FallbackBehavior<int>.Create(fallbackFunc, fallbackAsync);

			Assert.That(behavior.ExecutionMode, Is.EqualTo(FallbackExecutionMode.Sync | FallbackExecutionMode.Async));
			Assert.That(behavior.Fallback, Is.SameAs(fallbackFunc));
			Assert.That(behavior.AsyncFallback, Is.SameAs(fallbackAsync));
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

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_SyncFallback_Is_Set()
		{
			const string expectedValue = "fallback value";
			var behavior = FallbackBehavior<string>.Create(() => expectedValue);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_CancellationToken_SyncFallback_Is_Set()
		{
			const string expectedValue = "fallback value";
			var behavior = FallbackBehavior<string>.Create((_) => expectedValue);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public async Task Should_ToFallbackPolicy_Return_FallbackPolicy_When_AsyncFallback_Is_Set()
		{
			const string expectedValue = "async fallback value";
			var behavior = FallbackBehavior<string>.Create(() => Task.FromResult(expectedValue));
			var policy = behavior.ToFallbackPolicy();

			var result = await policy.HandleAsync<string>((_) => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public async Task Should_ToFallbackPolicy_Return_FallbackPolicy_When_CancellationToken_AsyncFallback_Is_Set()
		{
			const string expectedValue = "async fallback value";
			var behavior = FallbackBehavior<string>.Create((_) => Task.FromResult(expectedValue));
			var policy = behavior.ToFallbackPolicy();

			var result = await policy.HandleAsync<string>((_) => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_Both_Sync_And_Async_Fallbacks_Are_Set()
		{
			const string syncValue = "sync fallback value";
			const string asyncValue = "async fallback value";
			var behavior = FallbackBehavior<string>.Create(
				(_) => syncValue,
				(_) => Task.FromResult(asyncValue)
			);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(syncValue));
		}

		[Test]
		public async Task Should_ToFallbackPolicy_Return_FallbackPolicy_When_Both_Sync_And_Async_Async_Fallbacks_Are_Set()
		{
			const string syncValue = "sync fallback value";
			const string asyncValue = "async fallback value";
			var behavior = FallbackBehavior<string>.Create(
				(_) => syncValue,
				async (_) => await Task.FromResult(asyncValue)
			);
			var policy = behavior.ToFallbackPolicy();

			var result = await policy.HandleAsync<string>((_) => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(asyncValue));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_No_Fallback_Is_Set()
		{
			var behavior = FallbackBehavior<string>.Create((Func<string>)null);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_Only_AsyncFallback_Is_Null()
		{
			var behavior = FallbackBehavior<string>.Create(
				(_) => "sync value",
				null
			);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo("sync value"));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_Only_SyncFallback_Is_Null()
		{
			var behavior = FallbackBehavior<string>.Create(
				null,
				(CancellationToken _) => Task.FromResult("async value")
			);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo("async value"));
		}

		[Test]
		public void Should_ToFallbackPolicy_Work_With_Generic_Type()
		{
			var expectedValue = new CustomFallbackType { Id = 1, Name = "test" };
			var behavior = FallbackBehavior<CustomFallbackType>.Create(() => expectedValue);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<CustomFallbackType>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result.Id, Is.EqualTo(expectedValue.Id));
			Assert.That(result.Result.Name, Is.EqualTo(expectedValue.Name));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_SyncFallback_Returns_Default()
		{
			var behavior = FallbackBehavior<int>.Create(() => default(int), CancellationType.Precancelable);
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<int>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(0));
		}

		[Test]
		public void Should_ToFallbackPolicy_Return_FallbackPolicy_When_AsyncFallback_Returns_Default()
		{
			var behavior = FallbackBehavior<int>.Create((_) => Task.FromResult<int>(default));
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<int>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(0));
		}

		[Test]
		public void Should_ToFallbackPolicy_Handle_CancellationToken_SyncFallback()
		{
			var behavior = FallbackBehavior<int>.Create((ct) => ct.IsCancellationRequested ? -1 : 42);
			var policy = behavior.ToFallbackPolicy();

			using (var cts = new CancellationTokenSource())
			{
				var result = policy.Handle<int>(() => throw new Exception("test"), cts.Token);
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Errors, Is.Not.Null);
				Assert.That(result.Result, Is.EqualTo(42));
			}
		}

		[Test]
		public async Task Should_ToFallbackPolicy_HandleAsync_CancellationToken_AsyncFallback()
		{
			var behavior = FallbackBehavior<int>.Create((ct) => ct.IsCancellationRequested ? Task.FromResult(-1) : Task.FromResult(42));
			var policy = behavior.ToFallbackPolicy();

			using (var cts = new CancellationTokenSource())
			{
				var result = await policy.HandleAsync<int>((_) => throw new Exception("test"), cts.Token);

				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Errors, Is.Not.Null);
				Assert.That(result.Result, Is.EqualTo(42));
			}
		}

		[Test]
		public void Should_ToFallbackPolicy_Work_With_AsyncFallback_And_CancellationToken()
		{
			const string expectedValue = "async with ct value";
			var behavior = FallbackBehavior<string>.Create((_) => Task.FromResult(expectedValue));
			var policy = behavior.ToFallbackPolicy();

			var result = policy.Handle<string>(() => throw new Exception("test"));

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Errors, Is.Not.Null);
			Assert.That(result.Result, Is.EqualTo(expectedValue));
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

		private class CustomFallbackType
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
	}
}
