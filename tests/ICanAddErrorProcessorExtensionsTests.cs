using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class ICanAddErrorProcessorExtensionsTests
	{
		[Test]
		[TestCase(TestType.PolicyProc)]
		[TestCase(TestType.BulkErrorProc)]
		[TestCase(TestType.PolicyDelegateCol)]
		[TestCase(TestType.CatchBlockHandler)]
		[TestCase(TestType.PolicyDelegateColT)]
		[TestCase(TestType.PolicyCol)]
		public void Should_WithErrorProcessorOf_AddErrorProcessors(TestType testType)
		{
			int errorProcessorsCount = 1;
			IErrorProcessorRegistration v = null;

			if (testType == TestType.PolicyProc)
			{
				v = new PolicyProcessorErrorProcessorRegistration();
			}
			else if (testType == TestType.BulkErrorProc)
			{
				v = new BulkErrorProcessorErrorProcessorRegistration();
			}
			else if (testType == TestType.CatchBlockHandler)
			{
				v = new CatchBlockHandlerErrorProcessorRegistration();
			}
			else if (testType == TestType.PolicyDelegateCol)
			{
				v = new PolicyDelegateCollectionErrorProcessorRegistration();
			}
			else if (testType == TestType.PolicyDelegateColT)
			{
				v = new PolicyDelegateCollectionErrorProcessorRegistration<int>();
			}
			else
			{
				v = new PolicyCollectionErrorProcessorRegistration();
			}

			v.WithErrorProcessorOf((Exception _, CancellationToken __) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __, CancellationToken ___) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf((Exception _) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf((Exception _) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1));
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1));
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1));
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1));
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessor(new DefaultErrorProcessor());
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount++));

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.That(v.Count, Is.EqualTo(errorProcessorsCount));
		}

		[Test]
		public void Should_WithTypedErrorProcessor_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			var typedProcessor = new DefaultTypedErrorProcessor<InvalidOperationException>((_, __) => { });
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessor(typedProcessor, (p, ep) =>
			{
				capturedCanAdd = p;
				capturedErrorProcessor = ep;
			});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.SameAs(typedProcessor));
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __) => { },
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_CancellationType_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __) => { },
				CancellationType.Precancelable,
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_CancelTokenParam_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __, ___) => { },
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_AsyncFunc_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __) => Task.CompletedTask,
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_AsyncFunc_And_CancellationType_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __) => Task.CompletedTask,
				CancellationType.Precancelable,
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		[Test]
		public void Should_WithTypedErrorProcessorOf_With_AsyncFunc_And_CancelTokenParam_InvokeActionWithCorrectParameters()
		{
			// Arrange
			var processor = new SimplePolicyProcessor();
			ICanAddErrorProcessor capturedCanAdd = null;
			IErrorProcessor capturedErrorProcessor = null;

			// Act
			processor.WithTypedErrorProcessorOf<SimplePolicyProcessor, InvalidOperationException>(
				(_, __, ___) => Task.CompletedTask,
				(p, ep) =>
				{
					capturedCanAdd = p;
					capturedErrorProcessor = ep;
				});

			// Assert
			Assert.That(capturedCanAdd, Is.SameAs(processor));
			Assert.That(capturedErrorProcessor, Is.Not.Null);
			Assert.That(capturedErrorProcessor, Is.InstanceOf<DefaultTypedErrorProcessor<InvalidOperationException>>());
		}

		internal enum TestType
		{
			PolicyProc,
			BulkErrorProc,
			CatchBlockHandler,
			PolicyDelegateCol,
			PolicyDelegateColT,
			PolicyCol
		}
	}
}
