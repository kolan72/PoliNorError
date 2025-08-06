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
        [TestCase(PolicyAlias.Retry, true)]
        [TestCase(PolicyAlias.Retry, false)]
        [TestCase(PolicyAlias.Simple, true)]
        [TestCase(PolicyAlias.Simple, false)]
        public async Task Should_Respect_IsExecuted(PolicyAlias alias, bool isSync)
        {
            PolicyResult result = null;
            switch (alias)
            {
                case PolicyAlias.Fallback:
                    var fp = new DefaultFallbackProcessor();
                    if (isSync)
                    {
                        result = fp.Fallback(() => { }, (_) => { }, default);
                    }
                    else
                    {
                        result =await fp.FallbackAsync((_) => Task.CompletedTask, (_) => Task.CompletedTask, default);
                    }
                    break;
                case PolicyAlias.Retry:
                    var rp = new DefaultRetryProcessor();
                    if (isSync)
                    {
                        result = rp.Retry(() => { }, 1);
                    }
                    else
                    {
                        result = await rp.RetryAsync((_) => Task.CompletedTask, 1);
                    }
                    break;
                case PolicyAlias.Simple:
                    var sp = new SimplePolicyProcessor();
                    if (isSync)
                    {
                        result = sp.Execute(() => { });
                    }
                    else
                    {
                        result = await sp.ExecuteAsync((_) => Task.CompletedTask, 1);
                    }
                    break;
            }
            Assert.That(result._executed, Is.True);
        }

        [Test]
        [TestCase(PolicyAlias.Fallback, true)]
        [TestCase(PolicyAlias.Fallback, false)]
        [TestCase(PolicyAlias.Retry, true)]
        [TestCase(PolicyAlias.Retry, false)]
        [TestCase(PolicyAlias.Simple, true)]
        [TestCase(PolicyAlias.Simple, false)]
        public async Task Should_Respect_IsExecuted_ForGeneric(PolicyAlias alias, bool isSync)
        {
            PolicyResult<int> result = null;
            switch (alias)
            {
                case PolicyAlias.Fallback:
                    var fp = new DefaultFallbackProcessor();
                    if (isSync)
                    {
                        result = fp.Fallback(() => 1, (_) => 1, default);
                    }
                    else
                    {
                        result = await fp.FallbackAsync((_) => Task.FromResult(1), (_) => Task.FromResult(1), default);
                    }
                    break;
                case PolicyAlias.Retry:
                    var rp = new DefaultRetryProcessor();
                    if(isSync)
                    {
                        result = rp.Retry(() => 1, 1);
                    }
                    else
                    {
                        result = await rp.RetryAsync((_) => Task.FromResult(1), 1);
                    }
                    break;
                case PolicyAlias.Simple:
                    var sp = new SimplePolicyProcessor();
                    if (isSync)
                    {
                        result = sp.Execute(() => 1);
                    }
                    else
                    {
                        result = await sp.ExecuteAsync((_) => Task.FromResult(1));
                    }
                    break;
            }
            Assert.That(result._executed, Is.True);
        }

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnNone_WhenNoWrappedResultsExist(bool isGeneric)
		{
			if (isGeneric)
			{
				var policyResult = PolicyResult<int>.ForSync();
				policyResult.WrappedPolicyResults = null;
				policyResult.SetOk();  // Set status to non-NotExecuted
				Assert.That(policyResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.None));
			}
			else
			{
				var policyResult = PolicyResult.ForSync();
				policyResult.WrappedPolicyResults = null;
				policyResult.SetOk();  // Set status to non-NotExecuted
				Assert.That(policyResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.None));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnNone_WhenStatusIsNotNotExecutedAndWrappedPolicyResultsIsEmpty(bool isGeneric)
		{
			if (isGeneric)
			{
				var policyResult = PolicyResult<int>.ForSync();
				policyResult.WrappedPolicyResults = new List<PolicyDelegateResult<int>>();
				policyResult.SetOk(); // Makes Status = NoError
				Assert.That(policyResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.None));
			}
			else
			{
				var policyResult = PolicyResult.ForSync();
				policyResult.WrappedPolicyResults = new List<PolicyDelegateResult>();
				policyResult.SetOk(); // Makes Status = NoError
				Assert.That(policyResult.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.None));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnLastWrappedPolicyResultStatus_WhenMultipleWrappedPolicyResults(bool isGeneric)
		{
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr1 = new PolicyResult<int>();
				wrappedPr1.SetOk(); // First Wrapped Status = NoError

				var wrappedPr2 = new PolicyResult<int>();
				wrappedPr2.SetFailed(); // Second Wrapped Status = Failed

				var pdr1 = new PolicyDelegateResult<int>(wrappedPr1, "TestPolicy", null);
				var pdr2 = new PolicyDelegateResult<int>(wrappedPr2, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr1, pdr2 };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr1 = new PolicyResult();
				wrappedPr1.SetOk(); // First Wrapped Status = NoError

				var wrappedPr2 = new PolicyResult();
				wrappedPr2.SetFailed(); // Second Wrapped Status = Failed

				var pdr1 = new PolicyDelegateResult(wrappedPr1, "TestPolicy", null);
				var pdr2 = new PolicyDelegateResult(wrappedPr2, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr1, pdr2 };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_Return_NotExecuted_When_Status_Is_NotExecuted(bool isGeneric)
		{
			var result = isGeneric ? new PolicyResult<int>() : new PolicyResult();
			Assert.That(result.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.NotExecuted));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnNoError_WhenLastWrappedPolicyResultStatusIsNoError(bool isGeneric)
		{
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult<int>();
				wrappedPr.SetOk(); // Wrapped Status = NoError

				var pdr = new PolicyDelegateResult<int>(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.NoError));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult();
				wrappedPr.SetOk(); // Wrapped Status = NoError

				var pdr = new PolicyDelegateResult(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.NoError));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnCancel_WhenLastWrappedPolicyResultStatusIsCancel(bool isGeneric)
		{
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult<int>();
				wrappedPr.SetCanceled();

				var pdr = new PolicyDelegateResult<int>(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Canceled));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult();
				wrappedPr.SetCanceled();

				var pdr = new PolicyDelegateResult(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Canceled));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnFailed_WhenLastWrappedPolicyResultStatusIsFailed(bool isGeneric)
		{
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult<int>();
				wrappedPr.SetFailed();

				var pdr = new PolicyDelegateResult<int>(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult();
				wrappedPr.SetFailed();

				var pdr = new PolicyDelegateResult(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.Failed));
			}
		}

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Should_WrappedStatus_ReturnFailed_WhenLastWrappedPolicyResultStatusIsFailedWithCancellation(bool isGeneric)
        {
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult<int>();
				wrappedPr.SetFailedAndCanceled();

				var pdr = new PolicyDelegateResult<int>(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.FailedWithCancellation));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted

				var wrappedPr = new PolicyResult();
				wrappedPr.SetFailedAndCanceled();

				var pdr = new PolicyDelegateResult(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.FailedWithCancellation));
			}
		}

        [Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_WrappedStatus_ReturnPolicySuccess_WhenLastWrappedPolicyResultStatusIsPolicySuccess(bool isGeneric)
		{
			if (isGeneric)
			{
				// Arrange
				var pr = new PolicyResult<int>();
				pr.SetOk(); // Makes Status != NotExecuted
				var wrappedPr = new PolicyResult<int>();
				wrappedPr.SetExecuted();

				var pdr = new PolicyDelegateResult<int>(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult<int>> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.PolicySuccess));
			}
			else
			{
				// Arrange
				var pr = new PolicyResult();
				pr.SetOk(); // Makes Status != NotExecuted
				var wrappedPr = new PolicyResult();
				wrappedPr.SetExecuted();

				var pdr = new PolicyDelegateResult(wrappedPr, "TestPolicy", null);
				pr.WrappedPolicyResults = new List<PolicyDelegateResult> { pdr };

				// Act & Assert
				Assert.That(pr.WrappedStatus, Is.EqualTo(WrappedPolicyStatus.PolicySuccess));
			}
		}
	}
}