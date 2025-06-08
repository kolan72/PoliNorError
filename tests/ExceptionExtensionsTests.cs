using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class ExceptionExtensionsTests
	{
        [Test]
        public void Should_Return_New_OperationCanceledException_When_Not_Found()
        {
            // Arrange
            var aggEx = new AggregateException(new InvalidOperationException(), new Exception());

            // Act
            var result = aggEx.GetCancellationException();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<OperationCanceledException>());
        }

        [Test]
        public void Should_ReturnOperationCanceledException_WhenItIsTheOnlyException()
        {
            // Arrange
            var oce = new OperationCanceledException("Single OCE");
            var aggregateException = new AggregateException(oce);

            // Act
            var result = aggregateException.GetCancellationException();

            // Assert
            Assert.That(result, Is.SameAs(oce),
                "Expected the same OperationCanceledException instance to be returned.");
        }

        [Test]
        public void Should_ReturnTaskCanceledException_WhenItIsPresent()
        {
            // Arrange
            var tce = new TaskCanceledException("Task canceled");
            var aggregateException = new AggregateException(tce);

            // Act
            var result = aggregateException.GetCancellationException();

            // Assert
            Assert.That(result, Is.SameAs(tce),
                "Expected the same TaskCanceledException instance to be returned.");
            Assert.That(result, Is.TypeOf<TaskCanceledException>(),
                "Expected the exception type to remain TaskCanceledException.");
        }

        [Test]
        public void Should_ReturnFirstOperationCanceledException_WhenMultipleExist()
        {
            // Arrange
            var oce1 = new OperationCanceledException("First OCE");
            var oce2 = new OperationCanceledException("Second OCE");
            var aggregateException = new AggregateException(oce1, new InvalidOperationException("Other error"), oce2);

            // Act
            var result = aggregateException.GetCancellationException();

            // Assert
            Assert.That(result, Is.SameAs(oce1),
                "Expected the first OperationCanceledException to be returned.");
        }

        [Test]
        public void Should_ReturnOperationCanceledException_WhenMixedWithOtherExceptions()
        {
            // Arrange
            var oce = new OperationCanceledException("OCE among others");
            var otherEx = new ArgumentException("Argument error");
            var aggregateException = new AggregateException(otherEx, oce);

            // Act
            var result = aggregateException.GetCancellationException();

            // Assert
            Assert.That(result, Is.SameAs(oce),
                "Expected the OperationCanceledException to be returned among mixed exceptions.");
        }
    }
}
