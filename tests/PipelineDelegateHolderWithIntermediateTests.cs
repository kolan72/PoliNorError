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
	internal class PipelineDelegateHolderWithIntermediateTests
	{
        [Test]
        public void Should_CreateInstance_WithValidParameters()
        {
			// Arrange
			PipelineResult<string> prevFunc(int _, CancellationToken __) =>
				PipelineResult<string>.Success(PolicyResult<string>.ForSync());
			bool nextFunc(string _) => true;

			// Act
			var holder = new PipelineDelegateHolder<int, string, bool>(prevFunc, nextFunc);

            // Assert
            Assert.That(holder, Is.Not.Null);
        }

        [Test]
        public void Should_ReturnPipelineDelegate_WhenGetPipelineDelegateIsCalled()
        {
			// Arrange
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var result = PolicyResult<string>.ForSync();
				result.SetResult(input.ToString());
				result.SetOk();
				return PipelineResult<string>.Success(result);
			}
			bool nextFunc(string s) => s == "42";
			var holder = new PipelineDelegateHolder<int, string, bool>(prevFunc, nextFunc);

            // Act
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Assert
            Assert.That(pipelineDelegate, Is.Not.Null);
        }

        [Test]
        public void Should_ExecutePipelineSuccessfully_WhenBothStepsSucceed()
        {
			// Arrange
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var res = PolicyResult<string>.ForSync();
				res.SetResult(input.ToString());
				res.SetOk();
				return PipelineResult<string>.Success(res);
			}
			int nextFunc(string s) => s.Length;
			var holder = new PipelineDelegateHolder<int, string, int>(prevFunc, nextFunc);
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Act
            var result = pipelineDelegate(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(2));
        }

        [Test]
        public void Should_ReturnFailure_WhenPreviousFunctionFails()
        {
            // Arrange
            var failedResult = PolicyResult.ForSync();
            failedResult.SetFailedInner();

			PipelineResult<string> prevFunc(int _, CancellationToken __) =>
				PipelineResult<string>.Failure(failedResult);
			int nextFunc(string s) => s.Length;
			var holder = new PipelineDelegateHolder<int, string, int>(prevFunc, nextFunc);
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Act
            var result = pipelineDelegate(42, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.True);
        }

        [Test]
        public void Should_PropagateConfiguration_ToInnerDelegateHolder()
        {
			// Arrange
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var res = PolicyResult<string>.ForSync();
				res.SetResult(input.ToString());
				res.SetOk();
				return PipelineResult<string>.Success(res);
			}
			int nextFunc(string s) => s.Length;
			var holder = new PipelineDelegateHolder<int, string, int>(prevFunc, nextFunc);
            var configureWasCalled = false;
			void configure(BulkErrorProcessor _) { configureWasCalled = true; }

			// Act
			holder.SetConfigure(configure);
            var pipelineDelegate = holder.GetPipelineDelegate();
            pipelineDelegate(42, CancellationToken.None);

            // Assert
            Assert.That(configureWasCalled, Is.True);
        }

        [Test]
        public void Should_ChainMultipleTransformations_Successfully()
        {
			// Arrange
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var res = PolicyResult<string>.ForSync();
				res.SetResult($"Value: {input}");
				res.SetOk();
				return PipelineResult<string>.Success(res);
			}
			double nextFunc(string s) => s.Length * 1.5;
			var holder = new PipelineDelegateHolder<int, string, double>(prevFunc, nextFunc);
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Act
            var result = pipelineDelegate(100, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(15));
        }

        [Test]
        public void Should_HandleCancellationToken_InPipeline()
        {
            // Arrange
            using (var cts = new CancellationTokenSource())
            {
				PipelineResult<string> prevFunc(int input, CancellationToken _)
				{
					var res = PolicyResult<string>.ForSync();
					res.SetResult(input.ToString());
					res.SetOk();
					return PipelineResult<string>.Success(res);
				}
				bool nextFunc(string s) => !string.IsNullOrEmpty(s);
				var holder = new PipelineDelegateHolder<int, string, bool>(prevFunc, nextFunc);
                var pipelineDelegate = holder.GetPipelineDelegate();

                // Act
                var result = pipelineDelegate(123, cts.Token);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.IsFailed, Is.False);
            }
        }

        [Test]
        public void Should_AllowNullConfiguration_WithoutError()
        {
			// Arrange
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var res = PolicyResult<string>.ForSync();
				res.SetResult(input.ToString());
				res.SetOk();
				return PipelineResult<string>.Success(res);
			}
			int nextFunc(string s) => int.Parse(s);
			var holder = new PipelineDelegateHolder<int, string, int>(prevFunc, nextFunc);

            // Act
            holder.SetConfigure(null);
            var pipelineDelegate = holder.GetPipelineDelegate();
            var result = pipelineDelegate(99, CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(99));
        }

        [Test]
        public void Should_PreserveIntermediateType_InPipeline()
        {
            // Arrange
            string capturedIntermediate = null;
			PipelineResult<string> prevFunc(int input, CancellationToken _)
			{
				var res = PolicyResult<string>.ForSync();
				var intermediate = $"Processed-{input}";
				res.SetResult(intermediate);
				res.SetOk();
				return PipelineResult<string>.Success(res);
			}
			int nextFunc(string s)
			{
				capturedIntermediate = s;
				return s.Length;
			}
			var holder = new PipelineDelegateHolder<int, string, int>(prevFunc, nextFunc);
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Act
            var result = pipelineDelegate(5, CancellationToken.None);

            // Assert
            Assert.That(capturedIntermediate, Is.EqualTo("Processed-5"));
            Assert.That(result.Result, Is.EqualTo(11));
        }

        [Test]
        public void Should_HandleDifferentGenericTypes_Correctly()
        {
			// Arrange
			PipelineResult<int> prevFunc(string input, CancellationToken _)
			{
				var resu = PolicyResult<int>.ForSync();
				resu.SetResult(input.Length);
				resu.SetOk();
				return PipelineResult<int>.Success(resu);
			}
			bool nextFunc(int i) => i > 5;
			var holder = new PipelineDelegateHolder<string, int, bool>(prevFunc, nextFunc);
            var pipelineDelegate = holder.GetPipelineDelegate();

            // Act
            var result = pipelineDelegate("Testing", CancellationToken.None);

            // Assert
            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.True);
        }
    }
}
