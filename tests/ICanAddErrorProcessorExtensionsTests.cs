using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __, CancellationToken ___) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1));
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1));
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1));
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1));
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessor(new DefaultErrorProcessor());
			ClassicAssert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), CancellationType.Precancelable);
			ClassicAssert.AreEqual(errorProcessorsCount, v.Count);
		}

		internal enum TestType
		{
			PolicyProc,
			BulkErrorProc,
			PolicyDelegateCol,
			PolicyDelegateColT,
			PolicyCol
		}
	}
}
