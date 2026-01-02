using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static PoliNorError.BulkErrorProcessor;

namespace PoliNorError.Tests
{
	[TestFixture]
    public class BulkProcessResultTests
    {
        [Test]
        public void Should_InitializeWithHandlingError()
        {
            // Arrange
            var handlingError = new Exception("Test error");

            // Act
            var result = new BulkProcessResult(handlingError, null);

            // Assert
            Assert.That(result.HandlingError, Is.EqualTo(handlingError));
        }

        [Test]
        public void Should_InitializeWithEmptyProcessErrorsWhenNull()
        {
            // Arrange
            var handlingError = new Exception("Test error");

            // Act
            var result = new BulkProcessResult(handlingError, null);

            // Assert
            Assert.That(result.ProcessErrors, Is.Empty);
        }

        [Test]
        public void Should_InitializeWithProvidedProcessErrors()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"), null, ProcessStatus.Faulted),
                new ErrorProcessorException(new Exception("Error 2"), null, ProcessStatus.Faulted)
            };

            // Act
            var result = new BulkProcessResult(handlingError, processErrors);

            // Assert
            Assert.That(result.ProcessErrors, Has.Count.EqualTo(2));
            Assert.That(result.ProcessErrors, Is.EquivalentTo(processErrors));
        }

        [Test]
        public void Should_ReturnFalseForHasProcessErrorsWhenNoErrors()
        {
            // Arrange
            var handlingError = new Exception("Test error");

            // Act
            var result = new BulkProcessResult(handlingError, null);

            // Assert
            Assert.That(result.HasProcessErrors, Is.False);
        }

        [Test]
        public void Should_ReturnTrueForHasProcessErrorsWhenErrorsExist()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"), null, ProcessStatus.Faulted)
            };

            // Act
            var result = new BulkProcessResult(handlingError, processErrors);

            // Assert
            Assert.That(result.HasProcessErrors, Is.True);
        }

        [Test]
        public void Should_ReturnFalseForIsCanceledWhenNoErrorsAndNotCanceledBetweenProcessOne()
        {
            // Arrange
            var handlingError = new Exception("Test error");

            // Act
            var result = new BulkProcessResult(handlingError, null, false);

            // Assert
            Assert.That(result.IsCanceled, Is.False);
        }

        [Test]
        public void Should_ReturnTrueForIsCanceledWhenCanceledBetweenProcessOne()
        {
            // Arrange
            var handlingError = new Exception("Test error");

            // Act
            var result = new BulkProcessResult(handlingError, null, true);

            // Assert
            Assert.That(result.IsCanceled, Is.True);
        }

        [Test]
        public void Should_ReturnTrueForIsCanceledWhenProcessErrorHasCanceledStatus()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"), null, ProcessStatus.Canceled)
            };

            // Act
            var result = new BulkProcessResult(handlingError, processErrors, false);

            // Assert
            Assert.That(result.IsCanceled, Is.True);
        }

        [Test]
        public void Should_ReturnFalseForIsCanceledWhenProcessErrorsHaveNonCanceledStatus()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"), null, ProcessStatus.Faulted),
                new ErrorProcessorException(new Exception("Error 2"), null, ProcessStatus.Faulted)
            };

            // Act
            var result = new BulkProcessResult(handlingError, processErrors, false);

            // Assert
            Assert.That(result.IsCanceled, Is.False);
        }

        [Test]
        public void Should_ConvertProcessErrorsToCatchBlockExceptions()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"),  null, ProcessStatus.Faulted),
                new ErrorProcessorException(new Exception("Error 2"),  null, ProcessStatus.Faulted)
            };
            var result = new BulkProcessResult(handlingError, processErrors);

            // Act
            var catchBlockExceptions = result.ToCatchBlockExceptions().ToList();

            // Assert
            Assert.That(catchBlockExceptions, Has.Count.EqualTo(2));
            Assert.That(catchBlockExceptions[0].InnerException, Is.EqualTo(handlingError));
            Assert.That(catchBlockExceptions[1].InnerException, Is.EqualTo(handlingError));

            Assert.That(catchBlockExceptions[0].ProcessingException, Is.EqualTo(processErrors[0]));
            Assert.That(catchBlockExceptions[1].ProcessingException, Is.EqualTo(processErrors[1]));
        }

        [Test]
        public void Should_ReturnEmptyEnumerableFromToCatchBlockExceptionsWhenNoProcessErrors()
        {
            // Arrange
            var handlingError = new Exception("Test error");
            var result = new BulkProcessResult(handlingError, null);

            // Act
            var catchBlockExceptions = result.ToCatchBlockExceptions().ToList();

            // Assert
            Assert.That(catchBlockExceptions, Is.Empty);
        }

        [Test]
        public void Should_PreserveHandlingErrorInCatchBlockExceptions()
        {
            // Arrange
            var handlingError = new Exception("Original handling error");
            var processErrors = new List<ErrorProcessorException>
            {
                new ErrorProcessorException(new Exception("Error 1"), null, ProcessStatus.Faulted)
            };
            var result = new BulkProcessResult(handlingError, processErrors);

            // Act
            var catchBlockExceptions = result.ToCatchBlockExceptions();

            // Assert
            Assert.That(catchBlockExceptions.Count, Is.EqualTo(1));
            Assert.That(catchBlockExceptions.First().InnerException, Is.EqualTo(handlingError));
        }
    }
}
