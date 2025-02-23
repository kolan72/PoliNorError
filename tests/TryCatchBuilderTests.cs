using NUnit.Framework;
using PoliNorError.TryCatch;
using System;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class TryCatchBuilderTests
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CreateFrom_Really_Create(bool forAll)
		{
			ITryCatchBuilder tryCatchBuilder;
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
			Assert.That(tryCatch.HasCatchBlockForAll, Is.EqualTo(forAll));
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
			Assert.That(tryCatch.HasCatchBlockForAll, Is.EqualTo(forAll));
		}

		[Test]
		public void Should_AddCatchBlock_With_IBulkErrorProcessor_Param_Handle_Exception_Correctly()
		{
			var bulkErrorProcessor = new BulkErrorProcessor();
			int i = 0;
			bulkErrorProcessor.WithErrorProcessorOf((_) => i++);
			int m = 0;
			bulkErrorProcessor.WithErrorProcessorOf((_) => m++);
			var builder = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy
												(NonEmptyCatchBlockFilter.
													CreateByExcluding<InvalidOperationException>()))
							.AddCatchBlock(bulkErrorProcessor);

			var tryCatch = builder.Build();
			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);

			var result = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(result.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
			Assert.That(m, Is.EqualTo(1));
		}

		[Test]
		public void Should_AddCatchBlock_With_NonEmptyCatchBlockFilter_And_IBulkErrorProcessor_Params_Really_Add()
		{
			int i = 0;
			void act(Exception _)
			{
				i++;
			}

			var bulkErrorProcessor = new BulkErrorProcessor().WithErrorProcessorOf(act);
			var tryCatchBuilder =
				TryCatchBuilder
					.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()))
					.AddCatchBlock(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>(), bulkErrorProcessor);

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(2));

			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AddCatchBlock_With_NonEmptyCatchBlockFilter_Param_Really_Add(bool canHandle)
		{
			var tryCatchBuilder =
				TryCatchBuilder
					.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()))
					.AddCatchBlock(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(2));

			if (canHandle)
			{
				var res = tryCatch.Execute(() => throw new InvalidOperationException());
				Assert.That(res.IsError, Is.True);
			}
			else
			{
				var errorToThrow = new NotImplementedException("Test");
				var exc = Assert.Throws<NotImplementedException>(() => tryCatch.Execute(() => throw errorToThrow));
				Assert.That(exc, Is.EqualTo(errorToThrow));
			}
		}

		[Test]
		public void Should_CreateFrom_With_NonEmptyCatchBlockFilter_And_With_IBulkErrorProcessor_Param_Really_Create()
		{
			int i = 0;
			void act(Exception _)
			{
				i++;
			}

			var bulkErrorProcessor = new BulkErrorProcessor().WithErrorProcessorOf(act);
			var tryCatchBuilder =
				TryCatchBuilder
					.CreateFrom(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>(), bulkErrorProcessor);
			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));

			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		public void Should_CreateFrom_With_NonEmptyCatchBlockFilter_Param_Really_Create()
		{
			var tryCatchBuilder =
				TryCatchBuilder
					.CreateFrom(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));

			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
		}

		[Test]
		public void Should_CreateAndBuild_Create_ITryCatch_That_Handles_Exception()
		{
			var tryCatch = TryCatchBuilder.CreateAndBuild();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);

			var res = tryCatch.Execute(() => throw new Exception("Test"));
			Assert.That(res.IsError, Is.True);
		}

		[Test]
		public void Should_CreateAndBuild_With_IBulkErrorProcessor_Param_Create_ITryCatch_That_Handles_Exception()
		{
			var bulkErrorProcessor = new BulkErrorProcessor();
			int i = 0;
			bulkErrorProcessor.WithErrorProcessorOf((_) => i++);
			int m = 0;
			bulkErrorProcessor.WithErrorProcessorOf((_) => m++);
			var tryCatch = TryCatchBuilder.CreateAndBuild(bulkErrorProcessor);
			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);

			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
			Assert.That(m, Is.EqualTo(1));
		}

		[Test]
		public void Should_CreateAndBuild_With_Action_With_Exception_Param_Create_ITryCatch_That_Handles_Exception()
		{
			int i = 0;
			void act(Exception _)
			{
				i++;
			}

			var tryCatch = TryCatchBuilder.CreateAndBuild(act);
			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_CreateAndBuild_With_AsyncFunc_With_Exception_Param_Create_ITryCatch_That_Handles_Exception(bool isSync)
		{
			int i = 0;
			async Task fn(Exception _)
			{
				await Task.Delay(1);
				i++;
			}

			var tryCatch = TryCatchBuilder.CreateAndBuild(fn);

			TryCatchResult res;
			if (isSync)
			{
				res = tryCatch.Execute(() => throw new InvalidOperationException());
			}
			else
			{
				res = await tryCatch.ExecuteAsync(async(_) => { await Task.Delay(1); throw new InvalidOperationException(); });
			}

			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		public void Should_AddCatchBlock_With_NonEmptyCatchBlockFilter_Made_Of_Func_And_IBulkErrorProcessor_Configured_FiltersAndProcessesError()
		{
			int i = 0;
			void act(Exception _)
			{
				i++;
			}

			var tryCatchBuilder =
				TryCatchBuilder
					.CreateFrom(CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()))
					.AddCatchBlock((ef) => ef.IncludeError<InvalidOperationException>(), (b) => b.WithErrorProcessorOf(act));

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(2));

			var res = tryCatch.Execute(() => throw new InvalidOperationException());
			Assert.That(res.IsError, Is.True);
			Assert.That(i, Is.EqualTo(1));
		}
	}
}
