using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class ContextErrorProcessorsTests
	{
		[Test]
		public void Should_Add_ActionProcessor_And_ReturnSameInstance()
		{
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 3));
			var error = new Exception("test");
			var token = new CancellationTokenSource().Token;
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

		[Test]
		public void Should_Add_ActionProcessorWithCancellationType_And_AppendProcessor()
		{
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
			var errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 11));
			var error = new Exception("test");
			var token = new CancellationTokenSource().Token;
			bool invoked = false;

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

		[Test]
		public async Task Should_Add_AsyncProcessorWithCancellationType_And_AppendProcessor()
		{
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
			var sut = new ContextErrorProcessors<int>();
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
	}
}
