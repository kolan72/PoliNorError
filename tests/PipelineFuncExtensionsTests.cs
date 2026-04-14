using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    [TestFixture]
    public class PipelineFuncExtensionsTests
    {
        [Test]
        public void Should_Return_Success_When_Both_Functions_Succeed()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (input, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult(input.ToString());
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.True);
        }

        [Test]
        public void Should_Return_Failure_When_First_Function_Fails()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult.ForSync();
                policyResult.SetFailedInner();
                return PipelineResult<string>.Failure(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Result, Is.EqualTo(default(bool)));
        }

        [Test]
        public void Should_Return_Failure_When_Second_Function_Fails()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult("success");
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult.ForSync();
				policyResult.SetFailedInner();
				return PipelineResult<bool>.Failure(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Result, Is.EqualTo(default(bool)));
        }

        [Test]
        public void Should_Not_Execute_Second_Function_When_First_Function_Fails()
        {
            // Arrange
            var secondFuncExecuted = false;

            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult.ForSync();
                policyResult.SetFailedInner();
                return PipelineResult<string>.Failure(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				secondFuncExecuted = true;
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(secondFuncExecuted, Is.False);
        }

        [Test]
        public void Should_Pass_Result_From_First_Function_To_Second_Function()
        {
            // Arrange
            string capturedInput = null;

            Func<int, CancellationToken, PipelineResult<string>> func1 = (input, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult($"Value:{input}");
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<bool> func2(string input, CancellationToken _)
			{
				capturedInput = input;
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(capturedInput, Is.EqualTo("Value:42"));
        }

        [Test]
        public void Should_Pass_CancellationToken_To_Both_Functions()
        {
            // Arrange
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken capturedToken1 = default;
                CancellationToken capturedToken2 = default;

                Func<int, CancellationToken, PipelineResult<string>> func1 = (_, ct) =>
                {
                    capturedToken1 = ct;
                    var policyResult = PolicyResult<string>.ForSync();
                    policyResult.SetResult("test");
                    return PipelineResult<string>.Success(policyResult);
                };

				PipelineResult<bool> func2(string _, CancellationToken ct)
				{
					capturedToken2 = ct;
					var policyResult = PolicyResult<bool>.ForSync();
					policyResult.SetResult(true);
					return PipelineResult<bool>.Success(policyResult);
				}

				var boundFunc = func1.Bind(func2);

                // Act
                boundFunc(42, cts.Token);

                // Assert
                Assert.That(capturedToken1, Is.EqualTo(cts.Token));
                Assert.That(capturedToken2, Is.EqualTo(cts.Token));
            }
        }

        [Test]
        public void Should_Preserve_FailedPolicyResult_When_First_Function_Fails()
        {
            // Arrange
            var expectedFailedResult = PolicyResult.ForSync();
            expectedFailedResult.SetFailedInner(PolicyResultFailedReason.DelegateIsNull);

            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) => PipelineResult<string>.Failure(expectedFailedResult);

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.FailedPolicyResult, Is.SameAs(expectedFailedResult));
            Assert.That(result.FailedPolicyResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.DelegateIsNull));
        }

        [Test]
        public void Should_Preserve_FailedPolicyResult_When_Second_Function_Fails()
        {
            // Arrange
            var expectedFailedResult = PolicyResult.ForSync();
            expectedFailedResult.SetFailedInner(PolicyResultFailedReason.UnhandledError);

            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult("test");
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _) => PipelineResult<bool>.Failure(expectedFailedResult);

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.FailedPolicyResult, Is.SameAs(expectedFailedResult));
            Assert.That(result.FailedPolicyResult.FailedReason, Is.EqualTo(PolicyResultFailedReason.UnhandledError));
        }

        [Test]
        public void Should_Return_Correct_Result_Type_After_Binding()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult("test");
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<double> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult<double>.ForSync();
				policyResult.SetResult(3.14);
				return PipelineResult<double>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<PipelineResult<double>>());
            Assert.That(result.Result, Is.EqualTo(3.14));
        }

        [Test]
        public void Should_Handle_Cancellation_In_First_Function()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (__, _) =>
            {
                var policyResult = PolicyResult.ForSync();
                policyResult.SetFailedAndCanceled();
                return PipelineResult<string>.Failure(policyResult);
            };

			PipelineResult<bool> func2(string __, CancellationToken _)
			{
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(true);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.IsCanceled, Is.True);
        }

        [Test]
        public void Should_Chain_Multiple_Bind_Operations()
        {
            // Arrange
            Func<int, CancellationToken, PipelineResult<string>> func1 = (input, _) =>
            {
                var policyResult = PolicyResult<string>.ForSync();
                policyResult.SetResult(input.ToString());
                return PipelineResult<string>.Success(policyResult);
            };

			PipelineResult<int> func2(string input, CancellationToken _)
			{
				var policyResult = PolicyResult<int>.ForSync();
				policyResult.SetResult(input.Length);
				return PipelineResult<int>.Success(policyResult);
			}

			PipelineResult<bool> func3(int input, CancellationToken _)
			{
				var policyResult = PolicyResult<bool>.ForSync();
				policyResult.SetResult(input > 0);
				return PipelineResult<bool>.Success(policyResult);
			}

			var boundFunc = func1.Bind(func2).Bind(func3);

            // Act
            var result = boundFunc(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.True);
        }
    }
}
