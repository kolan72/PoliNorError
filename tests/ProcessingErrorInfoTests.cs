using NUnit.Framework;

namespace PoliNorError.Tests
{
	public class ProcessingErrorInfoTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_GetRetryCount_Return_Zero_For_Not_RetryProcessingErrorInfo(bool generic)
		{
			if (!generic)
			{
				var pi = new ProcessingErrorInfo(PolicyAlias.Simple, new ProcessingErrorContext(PolicyAlias.Simple));
				Assert.That(pi.GetRetryCount(), Is.Zero);
			}
			else
			{
				var pi = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, 2));
				Assert.That(pi.GetRetryCount(), Is.Zero);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_GetRetryCount_Return_NonZero_For_RetryProcessingErrorInfo(bool generic)
		{
			if (!generic)
			{
				var pi = new RetryProcessingErrorInfo(1);
				Assert.That(pi.GetRetryCount(), Is.EqualTo(1));
			}
			else
			{
				var pi = new RetryProcessingErrorInfo<int>(new RetryProcessingErrorContext<int>(1, 2));
				Assert.That(pi.GetRetryCount(), Is.EqualTo(1));
			}
		}
	}
}
