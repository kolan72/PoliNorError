using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class ErrorProcessorsTests
	{
		[Test]
		public void Should_Add_ActionProcessor_And_ReturnSameInstance()
		{
			var sut = new ErrorProcessors();
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
		public void Should_Add_ActionProcessorWithCancellationType_And_InvokeProcessor()
		{
			var sut = new ErrorProcessors();
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
		public void Should_Add_ActionProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var token = new CancellationTokenSource().Token;
			bool invoked = false;

			sut.Add((ex, ct) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(ct, Is.EqualTo(token));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error, cancellationToken: token);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessor_And_InvokeWithProcessAsync()
		{
			var sut = new ErrorProcessors();
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
		public async Task Should_Add_AsyncProcessorWithCancellationType_And_InvokeWithProcessAsync()
		{
			var sut = new ErrorProcessors();
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
		public async Task Should_Add_AsyncProcessorWithToken_And_UsePassedCancellationToken()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var token = new CancellationTokenSource().Token;
			bool invoked = false;

			sut.Add(async (ex, ct) =>
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

		[Test]
		public void Should_Add_ActionProcessorWithErrorInfo_And_PassErrorInfo()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			bool invoked = false;

			sut.AddWithInfo((ex, info) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Add_ActionProcessorWithErrorInfoAndToken_And_PassErrorInfoAndToken()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			var token = new CancellationTokenSource().Token;
			bool invoked = false;

			sut.AddWithInfo((ex, info, ct) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
				Assert.That(ct, Is.EqualTo(token));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error, errorInfo, token);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Add_ActionProcessorWithErrorInfoAndCancellationType_And_PassErrorInfo()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			bool invoked = false;

			sut.AddWithInfo((ex, info) =>
			{
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			sut.First().Process(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessorWithErrorInfo_And_InvokeWithProcessAsync()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			bool invoked = false;

			sut.AddWithInfo(async (ex, info) =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessorWithErrorInfoAndToken_And_PassErrorInfoAndToken()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			var token = new CancellationTokenSource().Token;
			bool invoked = false;

			sut.AddWithInfo(async (ex, info, ct) =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
				Assert.That(ct, Is.EqualTo(token));
			});

			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(error, errorInfo, cancellationToken: token);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public async Task Should_Add_AsyncProcessorWithErrorInfoAndCancellationType_And_InvokeWithProcessAsync()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
			bool invoked = false;

			sut.AddWithInfo(async (ex, info) =>
			{
				await Task.Delay(1);
				invoked = true;
				Assert.That(ex, Is.SameAs(error));
				Assert.That(info, Is.SameAs(errorInfo));
			}, CancellationType.Precancelable);

			Assert.That(sut.Count, Is.EqualTo(1));

			await sut.First().ProcessAsync(error, errorInfo);

			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_Enumerate_AddedProcessors_InInsertionOrder()
		{
			var sut = new ErrorProcessors();
			var error = new Exception("test");
			var markers = new List<int>();

			sut.Add(_ => markers.Add(1));
			sut.Add(_ => markers.Add(2), CancellationType.Precancelable);

			var processors = sut.ToList();

			processors[0].Process(error);
			processors[1].Process(error);

			Assert.That(sut.Count, Is.EqualTo(2));
			Assert.That(processors.Count, Is.EqualTo(2));
			Assert.That(ReferenceEquals(processors[0], processors[1]), Is.False);
			Assert.That(markers, Is.EqualTo(new[] { 1, 2 }));
		}
	}
}
