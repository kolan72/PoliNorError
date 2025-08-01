using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [Test]
        public void Should_PolicyStatus_Has_Correct_PolicyResultStatus()
        {
            Assert.That(PolicyStatus.NotExecuted.Status.Status, Is.EqualTo(0));
            Assert.That(PolicyStatus.NoError.Status.Status, Is.EqualTo(1));
            Assert.That(PolicyStatus.PolicySuccess.Status.Status, Is.EqualTo(2));
            Assert.That(PolicyStatus.Failed.Status.Status, Is.EqualTo(3));
            Assert.That(PolicyStatus.Canceled.Status.Status, Is.EqualTo(4));
            Assert.That(PolicyStatus.FailedWithCancellation.Status.Status, Is.EqualTo(5));
        }

        [Test]
        public void Should_WrappedPolicyStatus_Has_Correct_PolicyResultStatus()
        {
            Assert.That(WrappedPolicyStatus.NotExecuted.Status.Status, Is.EqualTo(0));
            Assert.That(WrappedPolicyStatus.NoError.Status.Status, Is.EqualTo(1));
            Assert.That(WrappedPolicyStatus.PolicySuccess.Status.Status, Is.EqualTo(2));
            Assert.That(WrappedPolicyStatus.Failed.Status.Status, Is.EqualTo(3));
            Assert.That(WrappedPolicyStatus.Canceled.Status.Status, Is.EqualTo(4));
            Assert.That(WrappedPolicyStatus.FailedWithCancellation.Status.Status, Is.EqualTo(5));

            Assert.That(WrappedPolicyStatus.None.Status.Status, Is.EqualTo(100));
        }

        [Test]
        public void Should_ResultStatusValue_ConstructCorrectly_FromWrappedPolicyResultStatusPart()
        {
            var status = new ResultStatusValue(WrappedPolicyResultStatusPart.None);
            Assert.That(status.Status, Is.EqualTo(100));
        }

        [Test]
        public void Should_ResultStatusValue_ConstructCorrectly_FromPolicyResultStatus()
        {
            var status = new ResultStatusValue(PolicyResultStatus.NotExecuted);
            Assert.That(status.Status, Is.EqualTo(0));
        }

        [Test]
        public void Should_ResultStatusValue_BeEqual_WhenSameStatusValue()
        {
            var status1 = new ResultStatusValue(1);
            var status2 = new ResultStatusValue(1);
            Assert.That(status1, Is.EqualTo(status2));
        }

        [Test]
        public void Should_ResultStatusValue_NotBeEqual_WhenDifferentStatusValues()
        {
            var status1 = new ResultStatusValue(1);
            var status2 = new ResultStatusValue(2);
            Assert.That(status1, Is.Not.EqualTo(status2));
        }

        [Test]
        public void Should_ResultStatusValue_OperatorEquals_ReturnTrue_ForEqualValues()
        {
            var status1 = new ResultStatusValue(3);
            var status2 = new ResultStatusValue(3);
            Assert.That(status1 == status2, Is.True);
        }

        [Test]
        public void Should_ResultStatusValue_OperatorNotEquals_ReturnTrue_ForDifferentValues()
        {
            var status1 = new ResultStatusValue(4);
            var status2 = new ResultStatusValue(5);
            Assert.That(status1 != status2, Is.True);
        }

        [Test]
        public void Should_Status_ReturnNotExecuted_WhenNoFlagsAreSet()
        {
            var policyResult = PolicyResult.ForSync();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.NotExecuted));
        }

        [Test]
        public void Should_Status_ReturnFailed_WhenOnlyIsFailedIsTrue()
        {
            var policyResult = PolicyResult.ForSync();
            policyResult.SetFailed();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.Failed));
        }

        [Test]
        public void Should_Status_ReturnFailedWithCancellation_WhenBothIsFailedAndIsCanceledAreTrue()
        {
            var policyResult = PolicyResult.ForSync();
            policyResult.SetFailedAndCanceled();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.FailedWithCancellation));
        }

        [Test]
        public void Should_Status_ReturnCanceled_WhenOnlyIsCanceledIsTrue()
        {
            var policyResult = PolicyResult.ForSync();
            policyResult.SetCanceled();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.Canceled));
        }

        [Test]
        public void Should_Status_ReturnNoError_WhenOnlyNoErrorIsTrue()
        {
            var policyResult = PolicyResult.ForSync();
            policyResult.SetOk();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.NoError));
        }

        [Test]
        public void Should_Status_ReturnPolicySuccess_WhenNoFlagsAreSet_And_Executed()
        {
            var policyResult = PolicyResult.ForSync();
            policyResult.SetExecuted();
            Assert.That(policyResult.Status, Is.SameAs(PolicyStatus.PolicySuccess));
        }

        [Test]
        [TestCase(PolicyAlias.Fallback, true)]
        [TestCase(PolicyAlias.Fallback, false)]
        public async Task Should_Respect_IsExecuted(PolicyAlias alias, bool isSync)
        {
            PolicyResult result = null;
            switch (alias)
            {
                case PolicyAlias.Fallback:
                    var rp = new DefaultFallbackProcessor();
                    if (isSync)
                    {
                        result = rp.Fallback(() => { }, (_) => { }, default);
                    }
                    else
                    {
                        result =await rp.FallbackAsync((_) => Task.CompletedTask, (_) => Task.CompletedTask, default);
                    }
                    break;
            }
            Assert.That(result._executed, Is.True);
        }

        [Test]
        [TestCase(PolicyAlias.Fallback, true)]
        [TestCase(PolicyAlias.Fallback, false)]
        public async Task Should_Respect_IsExecuted_ForGeneric(PolicyAlias alias, bool isSync)
        {
            PolicyResult<int> result = null;
            switch (alias)
            {
                case PolicyAlias.Fallback:
                    var rp = new DefaultFallbackProcessor();
                    if (isSync)
                    {
                        result = rp.Fallback(() => 1, (_) => 1, default);
                    }
                    else
                    {
                        result = await rp.FallbackAsync((_) => Task.FromResult(1), (_) => Task.FromResult(1), default);
                    }
                    break;
            }
            Assert.That(result._executed, Is.True);
        }
    }
}