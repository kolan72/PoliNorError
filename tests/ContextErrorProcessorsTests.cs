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
			var sut = new ContextErrorProcessors<int>();

			sut.Add((_, __) => { });
			sut.Add((_, __) => { }, CancellationType.Precancelable);

			var processors = sut.ToList();

			Assert.That(sut.Count, Is.EqualTo(2));
			Assert.That(processors.Count, Is.EqualTo(2));
			Assert.That(ReferenceEquals(processors[0], processors[1]), Is.False);
		}
	}
}
