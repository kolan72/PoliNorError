using NUnit.Framework;
using System;
using System.Linq;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class PolicyDelegateCollectionRegistrarTWithInnerErrorProcessorTests
	{
		[Test]
		public void Should_WithInnerErrorProcessor_Handle_Matching_Inner_Exception()
		{
			var processorCalled = false;
			var outerException = new InvalidOperationException("Outer", new InvalidOperationException("Inner"));

			var policyCollection = PolicyDelegateCollection<int>.Create()
				.WithSimple().AndDelegate(() => throw outerException)
				.WithInnerErrorProcessor(new DefaultInnerErrorProcessor<InvalidOperationException>((_, ___) => processorCalled = true));

			var result = policyCollection.BuildCollectionHandler().Handle();

			Assert.That(processorCalled, Is.True);
			Assert.That(result.LastPolicyResult.Errors.Any(), Is.True);
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Not_Handle_Non_Matching_Inner_Exception()
		{
			var processorCalled = false;
			var outerException = new InvalidOperationException("Outer", new ArgumentException("Inner"));

			var policyCollection = PolicyDelegateCollection<int>.Create()
				.WithSimple().AndDelegate(() => throw outerException)
				.WithInnerErrorProcessor(new DefaultInnerErrorProcessor<InvalidOperationException>((_, ___) => processorCalled = true));

			policyCollection.BuildCollectionHandler().Handle();

			Assert.That(processorCalled, Is.False);
		}

		[Test]
		public void Should_WithInnerErrorProcessor_Not_Handle_No_Inner_Exception()
		{
			var processorCalled = false;
			var outerException = new InvalidOperationException("Outer");

			var policyCollection = PolicyDelegateCollection<int>.Create()
				.WithSimple().AndDelegate(() => throw outerException)
				.WithInnerErrorProcessor(new DefaultInnerErrorProcessor<InvalidOperationException>((_, ___) => processorCalled = true));

			policyCollection.BuildCollectionHandler().Handle();

			Assert.That(processorCalled, Is.False);
		}

		[Test]
		public void Should_WithInnerErrorProcessor_On_Empty_Collection_Not_Throw()
		{
			var policyCollection = PolicyDelegateCollection<int>.Create();

			Assert.That(() => policyCollection.WithInnerErrorProcessor(new DefaultInnerErrorProcessor<InvalidOperationException>((_, ___) => { })), Throws.Nothing);
		}
	}
}

