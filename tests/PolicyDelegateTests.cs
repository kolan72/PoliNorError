using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace PoliNorError.Tests
{
	internal class PolicyDelegateTests
	{
        [Test]
        [TestCase(PolicyAlias.Simple, true)]
        [TestCase(PolicyAlias.Retry, true)]
        [TestCase(PolicyAlias.Fallback, true)]
        [TestCase(PolicyAlias.Simple, false)]
        [TestCase(PolicyAlias.Retry, false)]
        [TestCase(PolicyAlias.Fallback, false)]
        public void Should_HandleAsynchronousDelegate_WhenExecutingSyncWithAsyncDelegate(PolicyAlias policyAlias, bool isGeneric)
        {
            PolicyResult result = null;
            if (isGeneric)
            {
                var policyDelegate = GetPolicyByAlias(policyAlias).ToPolicyDelegate(async (_) => { await Task.Delay(TimeSpan.FromTicks(1)); return 1;});
                result = policyDelegate.Handle();
            }
            else
            {
                var policyDelegate = GetPolicyByAlias(policyAlias).ToPolicyDelegate(async (_) => await Task.CompletedTask);
                result = policyDelegate.Handle();
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
        }

        [Test]
        public void Should_Throw_NullReference_When_ExecutingSync_WithAsyncDelegate_ForPolicy_NotHandlingNullDelegates()
        {
            var tp = new TestPolicy();
            var policyDelegate = tp.ToPolicyDelegate(async (_) => await Task.CompletedTask);

            Assert.Throws<NullReferenceException>(() => policyDelegate.Handle());
        }

        [Test]
        public void Should_GenericPolicyDelegate_Throw_NullReference_When_ExecutingSync_WithAsyncDelegate_ForPolicy_NotHandlingNullDelegates()
        {
            var tp = new TestPolicy();
            var policyDelegate = tp.ToPolicyDelegate((_) => Task.FromResult(1));

            Assert.Throws<NullReferenceException>(() => policyDelegate.Handle());
        }

		[Test]
        [TestCase(PolicyAlias.Simple, true)]
        [TestCase(PolicyAlias.Retry, true)]
        [TestCase(PolicyAlias.Fallback, true)]
        [TestCase(PolicyAlias.Simple, false)]
        [TestCase(PolicyAlias.Retry, false)]
        [TestCase(PolicyAlias.Fallback, false)]
        public async Task Should_HandleSynchronousDelegate_WhenExecutingAsyncWithSyncDelegate(PolicyAlias policyAlias, bool isGeneric)
		{
            PolicyResult result = null;
            if (isGeneric)
            {
                var policyDelegate = GetPolicyByAlias(policyAlias).ToPolicyDelegate(() => 1);
                result = await policyDelegate.HandleAsync();
            }
            else
            {
                var policyDelegate = GetPolicyByAlias(policyAlias).ToPolicyDelegate(() => { });
                result = await policyDelegate.HandleAsync();
            }
			Assert.That(result, Is.Not.Null);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
		}

		[Test]
        public void Should_Throw_NullReference_When_ExecutingAsync_WithSyncDelegate_ForPolicy_NotHandlingNullDelegates()
        {
            var tp = new TestPolicy();
            var policyDelegate = tp.ToPolicyDelegate(() => { });

            Assert.ThrowsAsync<NullReferenceException>(async() => await policyDelegate.HandleAsync());
        }

        [Test]
        public void Should_GenericPolicyDelegate_Throw_NullReference_When_ExecutingAsync_WithSyncDelegate_ForPolicy_NotHandlingNullDelegates()
        {
            var tp = new TestPolicy();
            var policyDelegate = tp.ToPolicyDelegate(() => 1);

            Assert.ThrowsAsync<NullReferenceException>(async () => await policyDelegate.HandleAsync());
        }

        private IPolicyBase GetPolicyByAlias(PolicyAlias policyAlias)
        {
            switch (policyAlias)
            {
                case PolicyAlias.Simple: return new SimplePolicy();
                case PolicyAlias.Fallback: return new FallbackPolicy();
                case PolicyAlias.Retry: return new RetryPolicy(1);
                default: throw new NotImplementedException();
            }
        }

        internal class TestPolicy : IPolicyBase
        {
            public IPolicyProcessor PolicyProcessor => new SimplePolicyProcessor();

            public string PolicyName => nameof(TestPolicy);

            public PolicyResult Handle(Action action, CancellationToken token = default){action(); return PolicyResult.ForSync();}

			public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default) { func(); return PolicyResult<T>.ForSync(); }

            public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
            {
                await func(token);
                return PolicyResult.ForNotSync();
            }

            public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
            {
                var r = await func(token);
                var pr = PolicyResult<T>.ForNotSync();
                pr.SetResult(r);
                return pr;
            }
		}
	}
}
