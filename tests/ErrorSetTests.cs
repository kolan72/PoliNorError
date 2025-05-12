using NUnit.Framework;
using System;
using System.Linq;
using static PoliNorError.ErrorSet;

namespace PoliNorError.Tests
{
	public class ErrorSetTests
	{
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
			var esi = new ErrorSetItem(typeof(InvalidOperationException), ErrorSetItem.ItemType.Error);
			Assert.That(esi.Equals(null), Is.False);
		}
	}
}
