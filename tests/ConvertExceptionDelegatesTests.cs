using NUnit.Framework;
using System;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class ConvertExceptionDelegatesTests
	{
		// Custom exception for testing inheritance
#pragma warning disable RCS1194 // Implement exception constructors.
		private class CustomTestException : InvalidOperationException { }
#pragma warning disable S3871 // Exception types should be "public"
		private class UnrelatedException : Exception { }
#pragma warning restore S3871 // Exception types should be "public"
#pragma warning restore RCS1194 // Implement exception constructors.

		[Test]
        public void Should_ReturnTrue_When_ExceptionIsExactType()
        {
            // Arrange
            var original = new InvalidOperationException("Test message");

            // Act
            bool result = ConvertExceptionDelegates.ToSubException<InvalidOperationException>(original, out var typedException);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(typedException, Is.SameAs(original));
        }

        [Test]
        public void Should_ReturnTrue_When_ExceptionIsSubclass()
        {
            // Arrange
            var subclassEx = new CustomTestException();

            // Act - Checking if a CustomTestException can be treated as an InvalidOperationException
            bool result = ConvertExceptionDelegates.ToSubException<InvalidOperationException>(subclassEx, out var typedException);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(typedException, Is.Not.Null);
            Assert.That(typedException, Is.InstanceOf<CustomTestException>());
        }

        [Test]
        public void Should_ReturnFalse_When_ExceptionIsUnrelatedType()
        {
            // Arrange
            var unrelated = new UnrelatedException();

            // Act
            bool result = ConvertExceptionDelegates.ToSubException<ArgumentNullException>(unrelated, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnFalse_When_ExceptionIsNull()
        {
            // Arrange
            Exception nullEx = null;

            // Act
            bool result = ConvertExceptionDelegates.ToSubException<Exception>(nullEx, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }
    }
}
