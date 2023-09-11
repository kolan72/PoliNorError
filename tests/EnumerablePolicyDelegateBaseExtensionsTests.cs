using NUnit.Framework;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public class EnumerablePolicyDelegateBaseExtensionsTests
	{
		[Test]
		public void Should_AddPolicyResultHandlerForLastInner_DoesNotThrow_If_Empty()
		{
			var polDelegateCollectionWithHandlerFromAction = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromAction.AddPolicyResultHandlerForLastInner((_) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromAction.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithCancelType = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromActionWithCancelType.AddPolicyResultHandlerForLastInner((_) => { }, CancellationType.Precancelable);
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithCancelType.HandleAll());

			var polDelegateCollectionWithHandlerFromActionWithToken = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromActionWithToken.AddPolicyResultHandlerForLastInner((_, __) => { });
			Assert.DoesNotThrow(() => polDelegateCollectionWithHandlerFromActionWithToken.HandleAll());

			var polDelegateCollectionWithHandlerFromFunc = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFunc.AddPolicyResultHandlerForLastInner(async (_) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFunc.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithCancelType = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFuncWithCancelType.AddPolicyResultHandlerForLastInner(async (_) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithCancelType.HandleAllAsync());

			var polDelegateCollectionWithHandlerFromFuncWithToken = PolicyDelegateCollection.Create();
			polDelegateCollectionWithHandlerFromFuncWithToken.AddPolicyResultHandlerForLastInner(async (_, __) => await Task.Delay(1));
			Assert.DoesNotThrowAsync(async () => await polDelegateCollectionWithHandlerFromFuncWithToken.HandleAllAsync());
		}
	}
}
