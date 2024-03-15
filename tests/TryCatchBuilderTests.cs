using NUnit.Framework;
using PoliNorError.TryCatch;
using System;

namespace PoliNorError.Tests
{
	internal class TryCatchBuilderTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CreateFrom_Really_Create(bool forAll)
		{
			ITryCatchBuilder tryCatchBuilder = null;
			if (forAll)
			{
				tryCatchBuilder = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory.ForAllExceptions());
			}
			else
			{
				tryCatchBuilder = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()));
			}
			Assert.That(tryCatchBuilder, Is.TypeOf<TryCatchBuilder>());
			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddCatchBlock_Really_Add(bool forAll)
		{
			var tryCatchBuilder = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()));
			if (forAll)
			{
				tryCatchBuilder.AddCatchBlock(
					CatchBlockHandlerFactory.ForAllExceptions());
			}
			else
			{
				tryCatchBuilder.AddCatchBlock(
					CatchBlockHandlerFactory.FilterExceptionsBy(
						NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentNullException>()));
			}

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(2));
		}
	}
}
