using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal static class PolicyWithInnerErrorProcessorForTest
	{
		public static async Task Handle<T>(T policy, bool sync, bool withCancellationType) where T : IPolicyBase, IWithInnerErrorProcessor<T>
		{
			var innerProcessors = new InnerErrorProcessorFuncs();

			if (sync)
			{
				if (withCancellationType)
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable);
				}
				else
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action);
				}

				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				if (withCancellationType)
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable);
				}
				else
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc);
				}

				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.I, Is.EqualTo(1));

			if (sync)
			{
				policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken);
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken);
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.J, Is.EqualTo(1));

			if (sync)
			{
				if (withCancellationType)
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo);
				}
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				if (withCancellationType)
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo);
				}
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.K, Is.EqualTo(1));

			if (sync)
			{
				policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken);
				policy.Handle(ActionWithInner);
				policy.Handle(Action);
			}
			else
			{
				policy.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken);
				await policy.HandleAsync(AsyncFuncWithInner);
				await policy.HandleAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.L, Is.EqualTo(1));
		}
	}
}
