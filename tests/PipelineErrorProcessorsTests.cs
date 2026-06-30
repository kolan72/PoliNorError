using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class PipelineErrorProcessorsTests
	{
		[Test]
		public void Should_Add_ActionProcessor_And_ReturnSameInstance()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 7));
			var error = new Exception("test");
			bool invoked = false;

			var returned = sut.Add((ex, info) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info.Param, Is.EqualTo(7));
			});

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Add_ActionProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 3));
			var error = new Exception("test");
			using (var tokenSource = new CancellationTokenSource())
			{
				var token = tokenSource.Token;
				bool invoked = false;

				sut.Add((ex, info, ct) =>
				{
					invoked = true;
					Assert.That(ex, Is.SameAs(error));
					Assert.That(info.Param, Is.EqualTo(3));
					Assert.That(ct, Is.EqualTo(token));
				});

				Assert.That(sut.Count, Is.EqualTo(1));
				sut.First().Process(error, errorInfo, token);

				Assert.That(invoked, Is.True);
			}
		}

		[Test]
		public void Should_Add_ActionProcessorWithCancellationType_And_AppendProcessor()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 5));
			var error = new Exception("test");
			bool invoked = false;

			sut.Add((ex, info) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info.Param, Is.EqualTo(5));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));
			sut.First().Process(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessor_And_InvokeWithProcessAsync()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 9));
			var error = new Exception("test");
			bool invoked = false;

			sut.Add(async (ex, info) =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info.Param, Is.EqualTo(9));
			});

			Assert.That(sut.Count, Is.EqualTo(1));
			await sut.First().ProcessAsync(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 11));
			var error = new Exception("test");
			using (var tokenSource = new CancellationTokenSource())
			{
				bool invoked = false;

				var token = tokenSource.Token;

				sut.Add(async (ex, info, ct) =>
				{
					await Task.Delay(1);
					invoked = true;
					Assert.That(ex, Is.SameAs(error));
					Assert.That(info.Param, Is.EqualTo(11));
					Assert.That(ct, Is.EqualTo(token));
				});

				Assert.That(sut.Count, Is.EqualTo(1));
				await sut.First().ProcessAsync(error, errorInfo, cancellationToken: token);

				Assert.That(invoked, Is.True);
			}
		}

		[Test]
		public async Task Should_Add_AsyncProcessorWithCancellationType_And_AppendProcessor()
		{
			var sut = new PipelineErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 13));
			var error = new Exception("test");
			bool invoked = false;

			sut.Add(async (ex, info) =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info.Param, Is.EqualTo(13));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));
			await sut.First().ProcessAsync(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Enumerate_AddedProcessors_InInsertionOrder()
		{
			ProcessingErrorInfo<int> obj;
			var sut = new PipelineErrorProcessors<int>
			{
				(_,  pi) => obj = pi ,
				{(_) => { }, CancellationType.Precancelable }
			};

			var processors = sut.ToList();

			Assert.That(sut.Count, Is.EqualTo(2));
			Assert.That(processors.Count, Is.EqualTo(2));
			Assert.That(ReferenceEquals(processors[0], processors[1]), Is.False);
		}

		[Test]
		public void Should_Add_BasicActionProcessor_And_ReturnSameInstance()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			bool invoked = false;

			var returned = sut.Add(ex =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
			});

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Add_BasicActionProcessorWithCancellationType_And_InvokeProcessor()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			bool invoked = false;

			sut.Add(ex =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Add_BasicActionProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			using (var tokenSource = new CancellationTokenSource())
			{
				var token = tokenSource.Token;
				bool invoked = false;

				sut.Add((Exception ex, CancellationToken ct) =>
				{
					invoked = true;
					Assert.That(ex, Is.SameAs(error));
					Assert.That(ct, Is.EqualTo(token));
				});

				Assert.That(sut.Count, Is.EqualTo(1));

				sut.First().Process(error, cancellationToken: token);

				Assert.That(invoked, Is.True);
			}
		}

		[Test]
		public async Task Should_Add_BasicAsyncProcessor_And_InvokeWithProcessAsync()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			bool invoked = false;

			sut.Add(async ex =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(error);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_BasicAsyncProcessorWithCancellationType_And_InvokeWithProcessAsync()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			bool invoked = false;

			sut.Add(async ex =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(error);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_BasicAsyncProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new PipelineErrorProcessors<int>();
			var error = new Exception("test");
			using (var source = new CancellationTokenSource())
			{
				var token = source.Token;
				bool invoked = false;

				sut.Add(async (Exception ex, CancellationToken ct) =>
				{
					await Task.Delay(1);
					invoked = true;
					Assert.That(ex, Is.SameAs(error));
					Assert.That(ct, Is.EqualTo(token));
				});

				Assert.That(sut.Count, Is.EqualTo(1));

				await sut.First().ProcessAsync(error, cancellationToken: token);

				Assert.That(invoked, Is.True);
			}
		}

		#region Add(IErrorProcessor) Tests

		[Test]
		public void Should_Add_IErrorProcessor_And_ReturnSameInstance()
		{
			var sut = new PipelineErrorProcessors<int>();
			var processor = new TestErrorProcessor();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 42));

			var returned = sut.Add(processor);

			Assert.That(returned, Is.SameAs(sut));
			Assert.That(sut.Count, Is.EqualTo(1));
			sut.First().Process(error, errorInfo);
			Assert.That(processor.ProcessCalled, Is.True);
		}

		[Test]
		public void Should_Add_Multiple_IErrorProcessors_And_CountIncreasesCorrectly()
		{
			var sut = new PipelineErrorProcessors<string>();
			var proc1 = new TestErrorProcessor();
			var proc2 = new TestErrorProcessor();
			var proc3 = new TestErrorProcessor();

			sut.Add(proc1)
				.Add(proc2)
				.Add(proc3);

			Assert.That(sut.Count, Is.EqualTo(3));
			var allProcessors = sut.ToList();
			Assert.That(allProcessors[0], Is.SameAs(proc1));
			Assert.That(allProcessors[1], Is.SameAs(proc2));
			Assert.That(allProcessors[2], Is.SameAs(proc3));
		}

		[Test]
		public void Should_Add_IErrorProcessor_And_PassCorrectExceptionToProcessor()
		{
			var sut = new PipelineErrorProcessors<int>();
			var processor = new TestErrorProcessor();
			var expectedError = new InvalidOperationException("expected message");
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Retry, new ProcessingErrorContext<int>(PolicyAlias.Retry, 99));

			sut.Add(processor);
			sut.First().Process(expectedError, errorInfo);

			Assert.That(processor.ReceivedException, Is.SameAs(expectedError));
			Assert.That(processor.ReceivedContext, Is.SameAs(errorInfo));
		}

		[Test]
		public void Should_Add_IErrorProcessor_And_PassCancellationTokenToProcessor()
		{
			var sut = new PipelineErrorProcessors<object>();
			var processor = new TestErrorProcessor();
			var error = new Exception();
			using (var cts = new CancellationTokenSource())
			{
				var token = cts.Token;

				sut.Add(processor);
				sut.First().Process(error, cancellationToken: token);

				Assert.That(processor.ReceivedToken, Is.EqualTo(token));
			}
		}

		[Test]
		public void Should_Add_IErrorProcessor_And_ProcessInvokedOnIteration()
		{
			var sut = new PipelineErrorProcessors<double>();
			var processor = new TestErrorProcessor();

			sut.Add(processor);

			bool invokedDuringIteration = false;
			foreach (var p in sut)
			{
				p.Process(new Exception(), new ProcessingErrorInfo<double>(PolicyAlias.Simple, new ProcessingErrorContext<double>(PolicyAlias.Simple, 1.0)));
				invokedDuringIteration = true;
			}

			Assert.That(invokedDuringIteration, Is.True);
			Assert.That(processor.ProcessCalled, Is.True);
		}

		[Test]
		public void Should_Add_IErrorProcessor_And_MixedWithDelegateProcessors()
		{
			var sut = new PipelineErrorProcessors<long>();
			var processor = new TestErrorProcessor();
			long capturedParam = 0;

			sut.Add((Exception _, ProcessingErrorInfo<long> info) => capturedParam = info.Param)
				.Add(processor)
				.Add(_ => { });

			Assert.That(sut.Count, Is.EqualTo(3));

			var error = new Exception("mixed test");
			var errorInfo = new ProcessingErrorInfo<long>(PolicyAlias.Fallback, new ProcessingErrorContext<long>(PolicyAlias.Fallback, 7L));

			foreach (var p in sut)
			{
				p.Process(error, errorInfo);
			}

			Assert.That(capturedParam, Is.EqualTo(7L));
			Assert.That(processor.ProcessCalled, Is.True);
			Assert.That(processor.ReceivedException, Is.SameAs(error));
		}

		[Test]
		public async Task Should_Add_IErrorProcessor_And_InvokeProcessAsync()
		{
			var sut = new PipelineErrorProcessors<byte>();
			var processor = new TestErrorProcessor();
			var error = new Exception("async test");
			var errorInfo = new ProcessingErrorInfo<byte>(PolicyAlias.Simple, new ProcessingErrorContext<byte>(PolicyAlias.Simple, 0xAB));

			sut.Add(processor);
			await sut.First().ProcessAsync(error, errorInfo);

			Assert.That(processor.ProcessCalled, Is.True);
			Assert.That(processor.ReceivedException, Is.SameAs(error));
		}

		[Test]
		public async Task Should_Add_IErrorProcessor_And_PassCancellationTokenToProcessAsync()
		{
			var sut = new PipelineErrorProcessors<object>();
			var processor = new TestErrorProcessor();
			var error = new Exception();
			using (var cts = new CancellationTokenSource())
			{
				var token = cts.Token;

				sut.Add(processor);
				await sut.First().ProcessAsync(error, cancellationToken: token);

				Assert.That(processor.ReceivedToken, Is.EqualTo(token));
			}
		}

		[Test]
		public void Should_Add_IErrorProcessor_ThatThrows_And_ThrownCaughtByCaller()
		{
			var sut = new PipelineErrorProcessors<int>();
			var processor = new TestErrorProcessor();
			var thrownEx = new DivideByZeroException("processor failed");
			processor.SetThrow(thrownEx);

			sut.Add(processor);

			Assert.Throws<DivideByZeroException>(() => sut.First().Process(new Exception()));
		}

		private class TestErrorProcessor : IErrorProcessor
		{
			public bool ProcessCalled { get; private set; }
			public Exception ReceivedException { get; private set; }
			public ProcessingErrorInfo ReceivedContext { get; private set; }
			public CancellationToken ReceivedToken { get; private set; }

			public void SetThrow(Exception ex) => _throw = ex;

			private Exception _throw;

			public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
			{
				ProcessCalled = true;
				ReceivedException = error;
				ReceivedContext = catchBlockProcessErrorInfo;
				ReceivedToken = cancellationToken;

				if (_throw != null)
					throw _throw;

				if (cancellationToken.CanBeCanceled)
					cancellationToken.ThrowIfCancellationRequested();

				return error;
			}

			public Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				ProcessCalled = true;
				ReceivedException = error;
				ReceivedContext = catchBlockProcessErrorInfo;
				ReceivedToken = cancellationToken;

				if (_throw != null)
					throw _throw;

				if (cancellationToken.CanBeCanceled)
					cancellationToken.ThrowIfCancellationRequested();

				return Task.FromResult(error);
			}
		}

		#endregion
	}
}
