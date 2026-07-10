using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackFuncExtensionsTests
    {
        [Test]
        public void Should_PolicyResult_Contain_CatchBlockException_When_Exception_In_Fallback_Func()
        {
            var fallbackExc = new Exception("Test");
            var result = FallbackFuncExecResult.FromError(fallbackExc);
            var policyResult = PolicyResult.ForSync();
            result.ChangePolicyResult(policyResult, fallbackExc);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.FirstOrDefault()?.InnerException, Is.EqualTo(fallbackExc));
        }

        [Test]
        public void Should_Generic_PolicyResult_Contain_CatchBlockException_When_Exception_In_Generic_Fallback_Func()
        {
            var fallbackExc = new Exception("Test");
            var result = FallbackFuncExecResult<int>.FromError(fallbackExc);
            var policyResult = PolicyResult<int>.ForSync();
            result.ChangePolicyResult(policyResult, fallbackExc);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.FirstOrDefault()?.InnerException, Is.EqualTo(fallbackExc));
        }

        [Test]
        public void Should_PolicyResult_Contain_CatchBlockException_When_Cancellation_In_Fallback_Func()
        {
            var fallbackExc = new OperationCanceledException("Test");
            var result = FallbackFuncExecResult.FromCanceledError(fallbackExc);
            var policyResult = PolicyResult.ForSync();
            result.ChangePolicyResult(policyResult, fallbackExc);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.FirstOrDefault()?.InnerException, Is.EqualTo(fallbackExc));
        }

        [Test]
        public void Should_Generic_PolicyResult_Contain_CatchBlockException_When_Cancellation_In_Generic_Fallback_Func()
        {
            var fallbackExc = new OperationCanceledException("Test");
            var result = FallbackFuncExecResult<int>.FromCanceledError(fallbackExc);
            var policyResult = PolicyResult<int>.ForSync();
            result.ChangePolicyResult(policyResult, fallbackExc);
            Assert.That(policyResult.CatchBlockErrors.Count(), Is.EqualTo(1));
            Assert.That(policyResult.CatchBlockErrors.FirstOrDefault()?.InnerException, Is.EqualTo(fallbackExc));
        }
    }
}