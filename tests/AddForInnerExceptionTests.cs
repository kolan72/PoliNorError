using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class AddForInnerExceptionTests
	{
		[Test]
		public void Should_AddForInnerException_WithAction_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;

			var returned = sut.AddForInnerException<ArgumentException>(ex =>
			{
				invoked = true;
				capturedEx = ex;
			});

			Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
			Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

			sut.First().Process(outerException);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
		}

		[Test]
		public void Should_AddForInnerException_WithAction_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			bool invoked = false;

			sut.AddForInnerException<ArgumentException>(_ => invoked = true);

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(outerException);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public void Should_AddForInnerException_WithAction_NotProcessWhenNoInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var exception = new ArgumentException("No inner exception");
			bool invoked = false;

			sut.AddForInnerException<ArgumentException>(_ => invoked = true);

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(exception);

			Assert.That(invoked, Is.False, "Handler should not be invoked when there is no inner exception");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationType_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;

			var returned = sut.AddForInnerException<ArgumentException>(ex =>
			{
				invoked = true;
				capturedEx = ex;
			}, CancellationType.Precancelable);

			Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
			Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

			sut.First().Process(outerException);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationType_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			bool invoked = false;

			_ = sut.AddForInnerException<ArgumentException>(_ => invoked = true, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(outerException);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationType_NotProcessWhenPrecancelled()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;

			_ = sut.AddForInnerException<ArgumentException>(_ => invoked = true, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				sut.First().Process(outerException, cancellationToken: cts.Token);
			}

			Assert.That(invoked, Is.False, "Handler should not be invoked when already cancelled with Precancelable");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationToken_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;
			using (var cts = new CancellationTokenSource())
			{
				var returned = sut.AddForInnerException((ArgumentException ex, CancellationToken token) =>
				{
					invoked = true;
					capturedEx = ex;
					Assert.That(token, Is.EqualTo(cts.Token), "Should receive cancellation token");
				});

				Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
				Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

				sut.First().Process(outerException, cancellationToken: cts.Token);

				Assert.That(invoked, Is.True, "Handler should be invoked");
				Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
			}
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationToken_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			bool invoked = false;

			using (var cts = new CancellationTokenSource())
			{
				sut.AddForInnerException((ArgumentException _, CancellationToken __) => invoked = true);

				Assert.That(sut.Count, Is.EqualTo(1));

				sut.First().Process(outerException, cancellationToken: cts.Token);

				Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
			}
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndCancellationToken_NotProcessWhenNoInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var exception = new ArgumentException("No inner exception");
			bool invoked = false;
			using (var cts = new CancellationTokenSource())
			{
				sut.AddForInnerException((ArgumentException _, CancellationToken __) => invoked = true);

				Assert.That(sut.Count, Is.EqualTo(1));

				sut.First().Process(exception, cancellationToken: cts.Token);

				Assert.That(invoked, Is.False, "Handler should not be invoked when there is no inner exception");
			}
		}

		[Test]
		public async Task Should_AddForInnerException_WithFunc_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;

			var returned = sut.AddForInnerException<ArgumentException>(async ex =>
			{
				invoked = true;
				capturedEx = ex;
				await Task.CompletedTask;
			});

			Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
			Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

			await sut.First().ProcessAsync(outerException);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFunc_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			bool invoked = false;

			_ = sut.AddForInnerException<ArgumentException>(async _ =>
			  {
				  invoked = true;
				  await Task.CompletedTask;
			  });

			await sut.First().ProcessAsync(outerException);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFunc_NotProcessWhenNoInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var exception = new ArgumentException("No inner exception");
			bool invoked = false;

			_ = sut.AddForInnerException<ArgumentException>(async _ =>
			  {
				  invoked = true;
				  await Task.CompletedTask;
			  });

			await sut.First().ProcessAsync(exception);

			Assert.That(invoked, Is.False, "Handler should not be invoked when there is no inner exception");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndCancellationToken_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;

			using (var cts = new CancellationTokenSource())
			{
				var returned = sut.AddForInnerException(async (ArgumentException ex, CancellationToken token) =>
				{
					invoked = true;
					capturedEx = ex;
					Assert.That(token, Is.EqualTo(cts.Token), "Should receive cancellation token");
					await Task.CompletedTask;
				});

				Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
				Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

				await sut.First().ProcessAsync(outerException, cancellationToken: cts.Token);

				Assert.That(invoked, Is.True, "Handler should be invoked");
				Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
			}
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndCancellationToken_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			bool invoked = false;

			sut.AddForInnerException(async (ArgumentException _, CancellationToken __) => { invoked = true; await Task.CompletedTask; });

			await sut.First().ProcessAsync(outerException);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndCancellationType_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;
			ArgumentException capturedEx = null;

			var returned = sut.AddForInnerException<ArgumentException>(async ex =>
			{
				invoked = true;
				capturedEx = ex;
				await Task.CompletedTask;
			}, CancellationType.Precancelable);

			Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
			Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

			await sut.First().ProcessAsync(outerException);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
		}

		[Test]
		public void Should_AddForInnerException_WithFuncAndCancellationType_NotProcessWhenPrecancelled()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			bool invoked = false;

			sut.AddForInnerException<ArgumentException>(async _ => { invoked = true; await Task.CompletedTask; }, CancellationType.Precancelable);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.First().ProcessAsync(outerException, cancellationToken: cts.Token));
			}

			Assert.That(invoked, Is.False, "Handler should not be invoked when already cancelled with Precancelable");
		}
		[Test]
		public void Should_AddForInnerException_WithActionAndProcessingErrorInfo_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 42));
			bool invoked = false;
			ArgumentException capturedEx = null;
			ProcessingErrorInfo<int> capturedInfo = null;

			var returned = sut.AddForInnerException<ArgumentException>((ex, info) =>
			{
				invoked = true;
				capturedEx = ex;
				capturedInfo = info;
			});

			Assert.That(returned, Is.SameAs(sut), "Should return same instance for fluent chaining");
			Assert.That(sut.Count, Is.EqualTo(1), "Should add one processor");

			sut.First().Process(outerException, errorInfo);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException), "Handler should receive the inner exception");
			Assert.That(capturedInfo.Param, Is.EqualTo(42), "Handler should receive the ProcessingErrorInfo");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndProcessingErrorInfo_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 1));
			bool invoked = false;

			sut.AddForInnerException((ArgumentException _, ProcessingErrorInfo<int> __) => invoked = true);

			sut.First().Process(outerException, errorInfo);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationToken_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 7));
			bool invoked = false;

			using (var cts = new CancellationTokenSource())
			{
				var returned = sut.AddForInnerException<ArgumentException>((_, info, token) =>
				{
					invoked = true;
					Assert.That(token, Is.EqualTo(cts.Token));
					Assert.That(info.Param, Is.EqualTo(7));
				});

				Assert.That(returned, Is.SameAs(sut));

				sut.First().Process(outerException, errorInfo, cts.Token);

				Assert.That(invoked, Is.True, "Handler should be invoked");
			}
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationType_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 3));
			bool invoked = false;

			var returned = sut.AddForInnerException<ArgumentException>((_, info) =>
			{
				invoked = true;
				Assert.That(info.Param, Is.EqualTo(3));
			}, CancellationType.Precancelable);

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(outerException, errorInfo);

			Assert.That(invoked, Is.True, "Handler should be invoked");
		}

		[Test]
		public void Should_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationType_NotProcessWhenPrecancelled()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 1));
			bool invoked = false;

			sut.AddForInnerException<ArgumentException>((_, __) => invoked = true, CancellationType.Precancelable);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				sut.First().Process(outerException, errorInfo, cts.Token);
			}

			Assert.That(invoked, Is.False, "Handler should not be invoked when already cancelled with Precancelable");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndProcessingErrorInfo_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 99));
			bool invoked = false;
			ArgumentException capturedEx = null;

			var returned = sut.AddForInnerException<ArgumentException>(async (ex, info) =>
			{
				invoked = true;
				capturedEx = ex;
				Assert.That(info.Param, Is.EqualTo(99));
				await Task.CompletedTask;
			});

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(outerException, errorInfo);

			Assert.That(invoked, Is.True, "Handler should be invoked");
			Assert.That(capturedEx, Is.SameAs(innerException));
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndProcessingErrorInfo_NotProcessNonMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 1));
			bool invoked = false;

			sut.AddForInnerException(async (ArgumentException _, ProcessingErrorInfo<int> __) => { invoked = true; await Task.CompletedTask; });

			await sut.First().ProcessAsync(outerException, errorInfo);

			Assert.That(invoked, Is.False, "Handler should not be invoked for non-matching inner exception type");
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationToken_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 5));
			bool invoked = false;

			using (var cts = new CancellationTokenSource())
			{
				var returned = sut.AddForInnerException<ArgumentException>(async (_, info, token) =>
				{
					invoked = true;
					Assert.That(token, Is.EqualTo(cts.Token));
					Assert.That(info.Param, Is.EqualTo(5));
					await Task.CompletedTask;
				});

				Assert.That(returned, Is.SameAs(sut));

				await sut.First().ProcessAsync(outerException, errorInfo, cancellationToken: cts.Token);

				Assert.That(invoked, Is.True, "Handler should be invoked");
			}
		}

		[Test]
		public async Task Should_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationType_ProcessMatchingInnerException()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 11));
			bool invoked = false;

			var returned = sut.AddForInnerException<ArgumentException>(async (_, info) =>
			{
				invoked = true;
				Assert.That(info.Param, Is.EqualTo(11));
				await Task.CompletedTask;
			}, CancellationType.Precancelable);

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(outerException, errorInfo);

			Assert.That(invoked, Is.True, "Handler should be invoked");
		}

		[Test]
		public void Should_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationType_NotProcessWhenPrecancelled()
		{
			var sut = new PipelineErrorProcessors<int>();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 1));
			bool invoked = false;

			sut.AddForInnerException<ArgumentException>(async (_, __) => { invoked = true; await Task.CompletedTask; }, CancellationType.Precancelable);

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				Assert.ThrowsAsync<TaskCanceledException>(async () => await sut.First().ProcessAsync(outerException, errorInfo, cancellationToken: cts.Token));
			}

			Assert.That(invoked, Is.False, "Handler should not be invoked when already cancelled with Precancelable");
		}
	}
}
