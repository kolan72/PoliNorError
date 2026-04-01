using NUnit.Framework;
using System;
using System.Linq;

namespace PoliNorError.Tests
{
	public class ErrorSetTests
	{
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3376 // Attribute, EventArgs, and Exception type names should end with the type being extended
#pragma warning disable S3871 // Exception types should be "public"
		private class TestException1 : Exception { }
		private class TestException2 : Exception { }
		private class TestException3 : Exception { }
#pragma warning restore S3871 // Exception types should be "public"
#pragma warning restore S3376 // Attribute, EventArgs, and Exception type names should end with the type being extended
#pragma warning restore RCS1194 // Implement exception constructors.

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_FromError_Create_Instance_With_Error_Type(bool callWithErrorWithTheSameType)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>();
			if (callWithErrorWithTheSameType)
			{
				errorSet.WithError<ArgumentException>();
			}
			Assert.That(errorSet.Items.Count(), Is.EqualTo(1));
			Assert.That(errorSet.Items.FirstOrDefault().ErrorType, Is.EqualTo(typeof(ArgumentException)));
		}

		[Test]
		public void Should_WithError_Add_Error_Type()
		{
			var errorSet = ErrorSet.FromError<ArgumentException>();
			errorSet.WithError<ArgumentNullException>();
			Assert.That(errorSet.Items.Count(), Is.EqualTo(2));
			Assert.That(errorSet.Items.Skip(1).FirstOrDefault().ErrorType, Is.EqualTo(typeof(ArgumentNullException)));
		}

		[Test]
		public void Should_ErrorSetItem_Be_Equatable_With_Null()
		{
			var esi = new ErrorSet.ErrorSetItem(typeof(InvalidOperationException), ErrorSet.ErrorSetItem.ItemType.Error);
			Assert.That(esi.Equals(null), Is.False);
		}

        [Test]
        public void Should_ContainTwoErrorItems_When_CallingFromErrorsWithTwoGenericTypes()
        {
            // Act
            var result = ErrorSet.FromErrors<TestException1, TestException2>();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Has.Count.EqualTo(2));

            var expectedItem1 = new ErrorSet.ErrorSetItem(typeof(TestException1), ErrorSet.ErrorSetItem.ItemType.Error);
            var expectedItem2 = new ErrorSet.ErrorSetItem(typeof(TestException2), ErrorSet.ErrorSetItem.ItemType.Error);

            Assert.That(result.Items, Does.Contain(expectedItem1));
            Assert.That(result.Items, Does.Contain(expectedItem2));
        }

        [Test]
        public void Should_ContainThreeErrorItems_When_CallingFromErrorsWithThreeGenericTypes()
        {
            // Act
            var result = ErrorSet.FromErrors<TestException1, TestException2, TestException3>();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Has.Count.EqualTo(3));

            var expectedItem1 = new ErrorSet.ErrorSetItem(typeof(TestException1), ErrorSet.ErrorSetItem.ItemType.Error);
            var expectedItem2 = new ErrorSet.ErrorSetItem(typeof(TestException2), ErrorSet.ErrorSetItem.ItemType.Error);
            var expectedItem3 = new ErrorSet.ErrorSetItem(typeof(TestException3), ErrorSet.ErrorSetItem.ItemType.Error);

            Assert.That(result.Items, Does.Contain(expectedItem1));
            Assert.That(result.Items, Does.Contain(expectedItem2));
            Assert.That(result.Items, Does.Contain(expectedItem3));
        }

        [Test]
        public void Should_MarkAllItemsAsErrorKind_When_CallingFromErrors()
        {
            // Act
            var result = ErrorSet.FromErrors<TestException1, TestException2>();

            // Assert
            foreach (var item in result.Items)
            {
                Assert.That(item.ErrorKind, Is.EqualTo(ErrorSet.ErrorSetItem.ItemType.Error),
                    "FromErrors should only add items with ItemType.Error");
            }
        }

        [Test]
        public void Should_NotAddDuplicateItems_When_CallingFromErrorsWithSameTypeMultipleTimes()
        {
            // Act
            var result = ErrorSet.FromErrors<TestException1, TestException1, TestException1>();

            // Assert
            // HashSet implementation should prevent duplicates based on ErrorType and ErrorKind
            Assert.That(result.Items, Has.Count.EqualTo(1));

            var expectedItem = new ErrorSet.ErrorSetItem(typeof(TestException1), ErrorSet.ErrorSetItem.ItemType.Error);
            Assert.That(result.Items, Does.Contain(expectedItem));
        }

		[Test]
		public void Should_HasErrorGeneric_ReturnTrue_When_ErrorTypeExists()
		{
			var errorSet = ErrorSet.FromError<TestException1>();

			var result = errorSet.HasError<TestException1>();

			Assert.That(result, Is.True);
		}

		[Test]
		public void Should_HasErrorGeneric_ReturnFalse_When_ErrorTypeMissing()
		{
			var errorSet = ErrorSet.FromError<TestException1>();

			var result = errorSet.HasError<TestException2>();

			Assert.That(result, Is.False);
		}

		[Test]
		public void Should_HasInnerErrorGeneric_ReturnTrue_When_InnerErrorTypeExists()
		{
			var errorSet = ErrorSet.FromError<TestException1>()
				.WithInnerError<TestException2>();

			var result = errorSet.HasInnerError<TestException2>();

			Assert.That(result, Is.True);
		}

		[Test]
		public void Should_HasInnerErrorGeneric_ReturnFalse_When_InnerErrorTypeMissing()
		{
			var errorSet = ErrorSet.FromError<TestException1>()
				.WithInnerError<TestException2>();

			var result = errorSet.HasInnerError<TestException3>();

			Assert.That(result, Is.False);
		}
    }
}
