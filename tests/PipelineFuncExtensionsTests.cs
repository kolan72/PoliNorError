using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	public class PipelineFuncExtensionsTests
	{
        [Test]
        public void Should_Return_Success_When_Policy_Handles_Successfully_And_No_Cancellation()
        {
            // Arrange
            Func<int, string> func = x => x.ToString();
            var policy = new SimplePolicy();
            var cancellationToken = CancellationToken.None;

            // Act
            var wrappedFunc = func.ToHandledByPolicy(policy);
            var result = wrappedFunc(42, cancellationToken);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.IsCanceled, Is.False);
            Assert.That(result.Result, Is.EqualTo("42"));
            Assert.That(result.SucceededPolicyResult, Is.Not.Null);
        }

        [Test]
        public void Should_Return_Failure_When_CancellationToken_Is_Canceled()
		{
			// Arrange
			Func<int, string> func = x => x.ToString();
			var policy = new SimplePolicy();
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				// Act
				var wrappedFunc = func.ToHandledByPolicy(policy);
				var result = wrappedFunc(42, cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.True);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.FailedPolicyResult, Is.Not.Null);
				Assert.That(result.SucceededPolicyResult, Is.Null);
			}
		}
	}
}
