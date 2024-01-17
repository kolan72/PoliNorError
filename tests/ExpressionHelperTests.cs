using NUnit.Framework;
using System;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	public class ExpressionHelperTests
	{
		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_GetTypedInnerErrorFilter_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var bareFilterExpression = ExpressionHelper.GetTypedInnerErrorFilter<TestInnerException>();

			var bareExceptionWithInner = new TestExceptionWithInnerException();
			var exceptionThatSatisfyFilter = new Exception("", new TestInnerException("Test"));
			var exceptionThatNotSatisfyFilter = new Exception("", new TestInnerException("Test2"));

			if (withInnerError)
			{
				var filterExpression = ExpressionHelper.GetTypedInnerErrorFilter<TestInnerException>(ex => ex.Message == "Test");

				if (satisfyFilterFunc == true)
				{
					Assert.That(filterExpression.Compile()(exceptionThatSatisfyFilter), Is.True);
				}
				else if (satisfyFilterFunc == false)
				{
					Assert.That(filterExpression.Compile()(exceptionThatNotSatisfyFilter), Is.False);
				}
				else
				{
					Assert.That(bareFilterExpression.Compile()(bareExceptionWithInner), Is.True);
				}
			}
			else
			{
				Assert.That(bareFilterExpression.Compile()(new Exception()), Is.False);
			}
		}
	}
}
