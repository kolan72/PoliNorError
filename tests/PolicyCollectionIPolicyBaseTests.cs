using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionIPolicyBaseTests
	{
		[Test]
		public void Should_PolicyCollection_Implement_IPolicyBase()
		{
			var collection = PolicyCollection.Create(new RetryPolicy(1));

			Assert.That(collection, Is.InstanceOf<IPolicyBase>());
		}

		[Test]
		public void Should_PolicyProcessor_Return_Last_Policy_Processor()
		{
			var firstPolicy = new RetryPolicy(1);
			var lastPolicy = new SimplePolicy();
			var collection = PolicyCollection.Create().WithPolicy(firstPolicy).WithPolicy(lastPolicy);

			IPolicyBase policyBase = collection;

			Assert.That(policyBase.PolicyProcessor, Is.SameAs(lastPolicy.PolicyProcessor));
			Assert.That(policyBase.PolicyProcessor, Is.Not.SameAs(firstPolicy.PolicyProcessor));
		}

		[Test]
		public void Should_PolicyProcessor_Be_Null_For_Empty_PolicyCollection()
		{
			IPolicyBase policyBase = PolicyCollection.Create();

			Assert.That(policyBase.PolicyProcessor, Is.Null);
		}

		[Test]
		public void Should_PolicyName_Return_Type_Name_By_Default()
		{
			IPolicyBase policyBase = PolicyCollection.Create(new RetryPolicy(1));

			Assert.That(policyBase.PolicyName, Is.EqualTo(nameof(PolicyCollection)));
		}

		[Test]
		public void Should_PolicyName_Return_Set_Value()
		{
			var collection = PolicyCollection.Create(new RetryPolicy(1));

			var returned = collection.WithPolicyName("TestPolicyCollectionName");

			Assert.That(collection.PolicyName, Is.EqualTo("TestPolicyCollectionName"));
			Assert.That(returned, Is.SameAs(collection));
		}

		[Test]
		public void Should_Handle_Result_PolicyName_Equals_Collection_PolicyName()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");

			var result = collection.Handle(() => { });

			Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
		}

		[Test]
		public void Should_Handle_Result_PolicyName_Equals_Collection_PolicyName_By_Default()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			var result = collection.Handle(() => { });

			Assert.That(result.PolicyName, Is.EqualTo(nameof(PolicyCollection)));
		}

		[Test]
		public void Should_HandleT_Result_PolicyName_Equals_Collection_PolicyName()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");

			var result = collection.Handle(() => 42);

			Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public async Task Should_HandleAsync_Result_PolicyName_Equals_Collection_PolicyName()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");

			var result = await collection.HandleAsync(_ => Task.CompletedTask);

			Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
		}

		[Test]
		public async Task Should_HandleAsyncT_Result_PolicyName_Equals_Collection_PolicyName()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");

			var result = await collection.HandleAsync(_ => Task.FromResult(42));

			Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public void Should_Handle_Result_PolicyName_Equals_Collection_PolicyName_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var result = collection.Handle(() => { }, cts.Token);

				Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
			}
		}

		[Test]
		public async Task Should_HandleAsync_Result_PolicyName_Equals_Collection_PolicyName_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var result = await collection.HandleAsync(_ => Task.CompletedTask, cts.Token);

				Assert.That(result.PolicyName, Is.EqualTo("MyCollection"));
			}
		}

		[Test]
		public void Should_HandleDelegate_Results_Keep_Inner_Policy_Names()
		{
			var retryPolicy = new RetryPolicy(1).WithPolicyName("InnerRetry");
			var simplePolicy = new SimplePolicy().WithPolicyName("InnerSimple");
			var collection = PolicyCollection.Create().WithPolicy(retryPolicy).WithPolicy(simplePolicy).WithPolicyName("MyCollection");

			var collectionResult = collection.HandleDelegate(() => throw new Exception("Test"));
			var names = collectionResult.PolicyDelegateResults.Select(r => r.PolicyName).ToList();

			Assert.That(names, Does.Contain("InnerRetry"));
			Assert.That(names, Does.Contain("InnerSimple"));
			Assert.That(names, Has.No.Member("MyCollection"));
		}

		[Test]
		public void Should_Handle_And_HandleDelegate_Use_Different_PolicyName_Levels()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithPolicyName("MyCollection");

			var handleResult = collection.Handle(() => { });
			var delegateResult = collection.HandleDelegate(() => { });

			Assert.That(handleResult.PolicyName, Is.EqualTo("MyCollection"));
			Assert.That(delegateResult.LastPolicyResult.PolicyName, Is.EqualTo(nameof(RetryPolicy)));
		}

		[Test]
		public void Should_Handle_Return_LastPolicyResult_Of_HandleDelegate_For_Action()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			Action action = () => { };

			var resultViaHandle = collection.Handle(action);
			var resultViaHandleDelegate = collection.HandleDelegate(action).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsSuccess, Is.True);
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.IsSuccess, Is.EqualTo(resultViaHandleDelegate.IsSuccess));
			Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(resultViaHandleDelegate.IsCanceled));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public void Should_Handle_Return_LastPolicyResult_Of_HandleDelegate_For_Action_When_Policies_Fail()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			Action action = () => throw new Exception("Test");

			var resultViaHandle = collection.Handle(action);
			var resultViaHandleDelegate = collection.HandleDelegate(action).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsFailed, Is.True);
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public void Should_HandleT_Return_LastPolicyResult_Of_HandleDelegate_For_Func()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			Func<int> func = () => 42;

			var resultViaHandle = collection.Handle(func);
			var resultViaHandleDelegate = collection.HandleDelegate(func).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsSuccess, Is.True);
			Assert.That(resultViaHandle.Result, Is.EqualTo(42));
			Assert.That(resultViaHandle.Result, Is.EqualTo(resultViaHandleDelegate.Result));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public void Should_HandleT_Return_LastPolicyResult_Of_HandleDelegate_For_Func_When_Policies_Fail()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			Func<int> func = () => throw new Exception("Test");

			var resultViaHandle = collection.Handle(func);
			var resultViaHandleDelegate = collection.HandleDelegate(func).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsFailed, Is.True);
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_HandleAsync_Return_LastPolicyResult_Of_HandleDelegateAsync_For_AsyncFunc(bool configureAwait)
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			Func<CancellationToken, Task> func = _ => Task.CompletedTask;

			var resultViaHandle = await collection.HandleAsync(func, configureAwait);
			var resultViaHandleDelegate = (await collection.HandleDelegateAsync(func, configureAwait)).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsSuccess, Is.True);
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.IsSuccess, Is.EqualTo(resultViaHandleDelegate.IsSuccess));
			Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(resultViaHandleDelegate.IsCanceled));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public async Task Should_HandleAsync_Return_LastPolicyResult_Of_HandleDelegateAsync_For_AsyncFunc_When_Policies_Fail()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			Func<CancellationToken, Task> func = _ => throw new Exception("Test");

			var resultViaHandle = await collection.HandleAsync(func);
			var resultViaHandleDelegate = (await collection.HandleDelegateAsync(func)).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsFailed, Is.True);
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public async Task Should_HandleAsyncT_Return_LastPolicyResult_Of_HandleDelegateAsync_For_AsyncFuncT(bool configureAwait)
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			Func<CancellationToken, Task<int>> func = _ => Task.FromResult(42);

			var resultViaHandle = await collection.HandleAsync(func, configureAwait);
			var resultViaHandleDelegate = (await collection.HandleDelegateAsync(func, configureAwait)).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsSuccess, Is.True);
			Assert.That(resultViaHandle.Result, Is.EqualTo(42));
			Assert.That(resultViaHandle.Result, Is.EqualTo(resultViaHandleDelegate.Result));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public async Task Should_HandleAsyncT_Return_LastPolicyResult_Of_HandleDelegateAsync_For_AsyncFuncT_When_Policies_Fail()
		{
			var collection = PolicyCollection.Create().WithRetry(1).WithRetry(1);
			Func<CancellationToken, Task<int>> func = _ => throw new Exception("Test");

			var resultViaHandle = await collection.HandleAsync(func);
			var resultViaHandleDelegate = (await collection.HandleDelegateAsync(func)).LastPolicyResult;

			Assert.That(resultViaHandle, Is.Not.Null);
			Assert.That(resultViaHandle.IsFailed, Is.True);
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
			Assert.That(resultViaHandle.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaHandle.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public void Should_Handle_Via_IPolicyBase_Interface_Return_LastPolicyResult()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			Action action = () => { };

			var result = policyBase.Handle(action);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_HandleT_Via_IPolicyBase_Interface_Return_LastPolicyResult()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			Func<int> func = () => 42;

			var result = policyBase.Handle(func);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public async Task Should_HandleAsync_Via_IPolicyBase_Interface_Return_LastPolicyResult()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			Func<CancellationToken, Task> func = _ => Task.CompletedTask;

			var result = await policyBase.HandleAsync(func, false, CancellationToken.None);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public async Task Should_HandleAsyncT_Via_IPolicyBase_Interface_Return_LastPolicyResult()
		{
			IPolicyBase policyBase = PolicyCollection.Create().WithRetry(1);
			Func<CancellationToken, Task<int>> func = _ => Task.FromResult(42);

			var result = await policyBase.HandleAsync(func, false, CancellationToken.None);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(result.Result, Is.EqualTo(42));
		}

		[Test]
		public async Task Should_HandleAsync_Via_Extension_With_Token_Return_LastPolicyResult()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			Func<CancellationToken, Task> func = _ => Task.CompletedTask;

			var resultViaExtension = await collection.HandleAsync(func, CancellationToken.None);
			var resultViaHandleDelegate = (await collection.HandleDelegateAsync(func)).LastPolicyResult;

			Assert.That(resultViaExtension, Is.Not.Null);
			Assert.That(resultViaExtension.IsSuccess, Is.True);
			Assert.That(resultViaExtension.IsFailed, Is.EqualTo(resultViaHandleDelegate.IsFailed));
			Assert.That(resultViaExtension.FailedReason, Is.EqualTo(resultViaHandleDelegate.FailedReason));
		}

		[Test]
		public void Should_Handle_Return_LastPolicyResult_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var resultViaHandle = collection.Handle(() => { }, cts.Token);

				Assert.That(resultViaHandle, Is.Not.Null);
				Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(true));
				Assert.That(resultViaHandle.IsFailed, Is.EqualTo(false));
				Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
			}
		}

		[Test]
		public async Task Should_HandleAsync_Return_LastPolicyResult_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var resultViaHandle = await collection.HandleAsync((_) => Task.CompletedTask, cts.Token);

				Assert.That(resultViaHandle, Is.Not.Null);
				Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(true));
				Assert.That(resultViaHandle.IsFailed, Is.EqualTo(false));
				Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
			}
		}

		[Test]
		public async Task Should_HandleAsyncT_Return_LastPolicyResult_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var resultViaHandle = await collection.HandleAsync((_) => Task.FromResult(1), cts.Token);

				Assert.That(resultViaHandle, Is.Not.Null);
				Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(true));
				Assert.That(resultViaHandle.IsFailed, Is.EqualTo(false));
				Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
			}
		}

		[Test]
		public void Should_HandleT_Return_LastPolicyResult_When_Token_Is_Canceled()
		{
			var collection = PolicyCollection.Create().WithRetry(1);
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				var resultViaHandle = collection.Handle(() => 1, cts.Token);

				Assert.That(resultViaHandle, Is.Not.Null);
				Assert.That(resultViaHandle.IsCanceled, Is.EqualTo(true));
				Assert.That(resultViaHandle.IsFailed, Is.EqualTo(false));
				Assert.That(resultViaHandle.FailedReason, Is.EqualTo(PolicyResultFailedReason.None));
			}
		}

		[Test]
		public void Should_Handle_Return_Null_For_Null_Action()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			var result = collection.Handle((Action)null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void Should_HandleT_Return_Null_For_Null_Func()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			var result = collection.Handle((Func<int>)null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public async Task Should_HandleAsync_Return_Null_For_Null_Func()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			var result = await collection.HandleAsync((Func<CancellationToken, Task>)null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public async Task Should_HandleAsyncT_Return_Null_For_Null_Func()
		{
			var collection = PolicyCollection.Create().WithRetry(1);

			var result = await collection.HandleAsync((Func<CancellationToken, Task<int>>)null);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void Should_Handle_Return_Null_For_Empty_PolicyCollection()
		{
			var collection = PolicyCollection.Create();

			var result = collection.Handle(() => { });

			Assert.That(result, Is.Null);
		}
	}
}
