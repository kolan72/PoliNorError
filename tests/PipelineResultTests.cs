using NUnit.Framework;

namespace PoliNorError.Tests
{
    public class PipelineResultTests
    {
        [Test]
        public void Should_Create_Failed_PipelineResult_With_PolicyResult()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailed();

            // Act
            var result = PipelineResult<int>.Failure(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.IsCanceled, Is.False);
            Assert.That(result.Result, Is.EqualTo(default(int)));
        }

        [Test]
        public void Should_Create_Failed_PipelineResult_With_Canceled_PolicyResult()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailedAndCanceled();

            // Act
            var result = PipelineResult<string>.Failure(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.IsCanceled, Is.True);
            Assert.That(result.Result, Is.Null);
        }

        [Test]
        public void Should_Create_Failed_PipelineResult_With_Explicit_Cancellation_Flag()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailed();

            // Act
            var result = PipelineResult<double>.Failure(policyResult, isCanceled: true);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.IsCanceled, Is.True);
            Assert.That(result.Result, Is.EqualTo(default(double)));
        }

        [Test]
        public void Should_Create_Failed_PipelineResult_With_IsCanceled_False_When_Explicitly_Set()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailedAndCanceled();

            // Act
            var result = PipelineResult<int>.Failure(policyResult, isCanceled: false);

            // Assert
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.IsCanceled, Is.False);
        }

        [Test]
        public void Should_Create_Success_PipelineResult_With_Result_Value()
        {
            // Arrange
            var policyResult = new PolicyResult<int>();
            policyResult.SetOk();
            policyResult.SetResult(42);

            // Act
            var result = PipelineResult<int>.Success(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.IsCanceled, Is.False);
            Assert.That(result.Result, Is.EqualTo(42));
        }

        [Test]
        public void Should_Create_Success_PipelineResult_With_Reference_Type()
        {
            // Arrange
            var expectedValue = "test string";
            var policyResult = new PolicyResult<string>();
            policyResult.SetOk();
            policyResult.SetResult(expectedValue);

            // Act
            var result = PipelineResult<string>.Success(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Should_Return_Default_Result_When_Failed()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailed();

            // Act
            var result = PipelineResult<int>.Failure(policyResult);

            // Assert
            Assert.That(result.Result, Is.EqualTo(0));
        }

        [Test]
        public void Should_Return_Null_For_Reference_Type_When_Failed()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailed();

            // Act
            var result = PipelineResult<string>.Failure(policyResult);

            // Assert
            Assert.That(result.Result, Is.Null);
        }

        [Test]
        public void Should_Have_IsFailed_True_When_SucceededPolicyResult_Is_Null()
        {
            // Arrange
            var policyResult = new PolicyResult();

            // Act
            var result = PipelineResult<int>.Failure(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.True);
        }

        [Test]
        public void Should_Have_IsFailed_False_When_SucceededPolicyResult_Is_Not_Null()
        {
            // Arrange
            var policyResult = new PolicyResult<bool>();
            policyResult.SetResult(true);

            // Act
            var result = PipelineResult<bool>.Success(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.False);
        }

        [Test]
        public void Should_Preserve_IsCanceled_From_PolicyResult()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetCanceled();

            // Act
            var result = PipelineResult<int>.Failure(policyResult);

            // Assert
            Assert.That(result.IsCanceled, Is.True);
        }

        [Test]
        public void Should_Not_Set_IsCanceled_For_Success_Result()
        {
            // Arrange
            var policyResult = new PolicyResult<int>();
            policyResult.SetResult(100);

            // Act
            var result = PipelineResult<int>.Success(policyResult);

            // Assert
            Assert.That(result.IsCanceled, Is.False);
        }

        [Test]
        public void Should_Handle_Nullable_Value_Type()
        {
            // Arrange
            var policyResult = new PolicyResult<int?>();
            policyResult.SetResult(null);

            // Act
            var result = PipelineResult<int?>.Success(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.Null);
        }

        [Test]
        public void Should_Return_Correct_Result_For_Custom_Object()
        {
            // Arrange
            var customObject = new TestClass { Value = "test" };
            var policyResult = new PolicyResult<TestClass>();
            policyResult.SetResult(customObject);

            // Act
            var result = PipelineResult<TestClass>.Success(policyResult);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.SameAs(customObject));
            Assert.That(result.Result.Value, Is.EqualTo("test"));
        }

        [Test]
        public void Should_Override_PolicyResult_IsCanceled_When_Explicitly_Provided()
        {
            // Arrange
            var policyResult = new PolicyResult();
            policyResult.SetFailed();
            // Note: policyResult.IsCanceled is false

            // Act
            var result = PipelineResult<int>.Failure(policyResult, isCanceled: true);

            // Assert
            Assert.That(result.IsCanceled, Is.True);
        }

        private class TestClass
        {
            public string Value { get; set; }
        }
    }
}