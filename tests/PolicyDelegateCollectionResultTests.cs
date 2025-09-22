using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError.Tests
{
    public class PolicyDelegateCollectionResultTests
    {
        [Test]
        public void Should_Initialize_With_Empty_Collections_When_No_Parameters()
        {
            var result = new PolicyDelegateCollectionResult(
                Enumerable.Empty<PolicyDelegateResult>(),
                Enumerable.Empty<PolicyDelegate>()
            );

            Assert.That(result.PolicyDelegateResults, Is.Empty);
            Assert.That(result.PolicyDelegatesUnused, Is.Empty);
            Assert.That(result.LastPolicyResult, Is.Null);
        }

        [Test]
        public void Should_Set_Properties_From_Last_Result_When_Collection_Not_Empty()
        {
            var successResult = PolicyResult.ForSync();
            var failedResult = PolicyResult.ForSync();
            failedResult.SetFailedInner();

            var results = new List<PolicyDelegateResult>
            {
                new PolicyDelegateResult(successResult, null, null),
                new PolicyDelegateResult(failedResult, null, null)
            };

            var unusedDelegates = new List<PolicyDelegate> { new SimplePolicy().ToPolicyDelegate()};
            var collectionResult = new PolicyDelegateCollectionResult(results, unusedDelegates);

            Assert.That(collectionResult.IsFailed, Is.True);
            Assert.That(collectionResult.IsSuccess, Is.False);
            Assert.That(collectionResult.IsCanceled, Is.False);
            Assert.That(collectionResult.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
            Assert.That(collectionResult.PolicyDelegateResults, Is.EqualTo(results));
            Assert.That(collectionResult.PolicyDelegatesUnused, Is.EqualTo(unusedDelegates));
            Assert.That(collectionResult.LastPolicyResult, Is.EqualTo(results[results.Count-1].Result));
        }

        [Test]
        public void Should_Set_Success_When_Last_Result_Successful()
        {
            var successResult = PolicyResult.ForSync();
            var results = new List<PolicyDelegateResult> { new PolicyDelegateResult(successResult, null, null) };

			var collectionResult = new PolicyDelegateCollectionResult(results, new List<PolicyDelegate>());

            Assert.That(collectionResult.IsSuccess, Is.True);
            Assert.That(collectionResult.IsFailed, Is.False);
            Assert.That(collectionResult.IsCanceled, Is.False);
            Assert.That(collectionResult.LastPolicyResult, Is.EqualTo(results[0].Result));
        }

        [Test]
        public void Should_Set_Canceled_When_Last_Result_Canceled()
        {
            var canceledResult = PolicyResult.ForSync();
            canceledResult.SetCanceled();

            var results = new List<PolicyDelegateResult> { new PolicyDelegateResult(canceledResult, null, null) };

			var collectionResult = new PolicyDelegateCollectionResult(results, new List<PolicyDelegate>());

            Assert.That(collectionResult.IsCanceled, Is.True);
            Assert.That(collectionResult.IsSuccess, Is.False);
            Assert.That(collectionResult.IsFailed, Is.False);
        }

        [Test]
        public void Should_Use_LastPolicyResultState_When_Collection_Empty()
        {
            var lastState = LastPolicyResultState.FromFailed();
            var unusedDelegates = new List<PolicyDelegate> { new SimplePolicy().ToPolicyDelegate() };

            var collectionResult = new PolicyDelegateCollectionResult(
                Enumerable.Empty<PolicyDelegateResult>(),
                unusedDelegates,
                lastState,
                PolicyResultFailedReason.PolicyProcessorFailed
            );

            Assert.That(collectionResult.IsFailed, Is.True);
            Assert.That(collectionResult.IsSuccess, Is.False);
            Assert.That(collectionResult.LastPolicyResultFailedReason, Is.EqualTo(PolicyResultFailedReason.PolicyProcessorFailed));
            Assert.That(collectionResult.PolicyDelegateResults, Is.Empty);
            Assert.That(collectionResult.PolicyDelegatesUnused, Is.EqualTo(unusedDelegates));
            Assert.That(collectionResult.LastPolicyResult, Is.Null);
        }

        [Test]
        public void Should_Enumerate_PolicyDelegateResults()
        {
            var successResult = PolicyResult.ForSync();
            var results = new List<PolicyDelegateResult>
            {
                new PolicyDelegateResult(successResult, null, null),
                new PolicyDelegateResult(successResult, null, null)
            };

			var collectionResult = new PolicyDelegateCollectionResult(results, new List<PolicyDelegate>());
            var enumeratedResults = collectionResult.ToList();

            Assert.That(enumeratedResults, Has.Count.EqualTo(2));
            Assert.That(enumeratedResults, Is.EqualTo(results));
        }
    }
}
