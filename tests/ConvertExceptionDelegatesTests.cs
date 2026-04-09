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
            bool result = ConvertExceptionDelegates.TryCast<InvalidOperationException>(original, out var typedException);

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
            bool result = ConvertExceptionDelegates.TryCast<InvalidOperationException>(subclassEx, out var typedException);

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
            bool result = ConvertExceptionDelegates.TryCast<ArgumentNullException>(unrelated, out var typedException);

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
            bool result = ConvertExceptionDelegates.TryCast<Exception>(nullEx, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnTrue_WhenExceptionTypeMatchesExactly()
        {
            // Arrange
            var exception = new ArgumentException("Test message");

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<ArgumentException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(typedException, Is.Not.Null);
            Assert.That(typedException, Is.SameAs(exception));
        }

        [Test]
        public void Should_ReturnFalse_WhenExceptionTypeIsDerived()
        {
			// Arrange
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var exception = new ArgumentNullException("paramName");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

			// Act
			var result = ConvertExceptionDelegates.TryAsExact<ArgumentException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnFalse_WhenExceptionTypeIsBase()
        {
            // Arrange
            var exception = new ArgumentException("Test message");

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<Exception>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnFalse_WhenExceptionIsNull()
        {
            // Arrange
            Exception exception = null;

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<ArgumentException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnFalse_WhenExceptionTypeIsCompletelyDifferent()
        {
            // Arrange
            var exception = new InvalidOperationException("Test message");

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<ArgumentException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

        [Test]
        public void Should_ReturnTrue_WhenUsingCustomExceptionType()
        {
            // Arrange
            var exception = new CustomException();

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<CustomException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(typedException, Is.Not.Null);
            Assert.That(typedException, Is.SameAs(exception));
        }

        [Test]
        public void Should_ReturnFalse_WhenCustomExceptionIsDerived()
        {
            // Arrange
            var exception = new DerivedCustomException();

            // Act
            var result = ConvertExceptionDelegates.TryAsExact<CustomException>(exception, out var typedException);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(typedException, Is.Null);
        }

		// Helper classes for testing
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3871 // Exception types should be "public"
		private class CustomException : Exception
		{
        }

        private class DerivedCustomException : CustomException
        {
        }
#pragma warning restore S3871 // Exception types should be "public"
#pragma warning restore RCS1194 // Implement exception constructors.
    }
}
