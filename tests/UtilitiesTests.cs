using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

namespace PoliNorError.Tests
{
	internal class UtilitiesTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Exception_DataContainsKeyStringWithValue_Returns_CorrectBoolValue_ForAnyGenericType(bool sameType)
		{
			var exc = new Exception("Test");
			exc.Data["TestBool"] = true;
			if (sameType)
			{
				Assert.That(exc.DataContainsKeyStringWithValue("TestBool", true), Is.True);
			}
			else
			{
				Assert.That(exc.DataContainsKeyStringWithValue("TestBool", 5), Is.False);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ThrowHelper_ThrowIfNotImplemented_Throw_Or_Return_Correctly(bool notImplemented)
		{
			if (notImplemented)
			{
				ISimplePolicyProcessor processor = new SimplePolicyTests.TestSimplePolicyProcessor();
				var exc = Assert.Throws<NotImplementedException>(() => ThrowHelper.ThrowIfNotImplemented(processor, out SimplePolicyProcessor impl));
				Assert.That(exc.Message, Is.EqualTo("Supported only for the SimplePolicyProcessor implementation of the ISimplePolicyProcessor."));
			}
			else
			{
				ISimplePolicyProcessor processor = new SimplePolicyProcessor();
				ThrowHelper.ThrowIfNotImplemented(processor, out SimplePolicyProcessor impl);
				Assert.That(impl, Is.Not.Null);
			}
		}

#pragma warning disable S2094 // Classes should not be empty
		private class TestClass { }
#pragma warning restore S2094 // Classes should not be empty

	}
}