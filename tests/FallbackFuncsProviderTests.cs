using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FallbackFuncsProviderTests
	{
		[Test]
		[TestCase(TestFallbackFuncType.NoFuncs)]
		[TestCase(TestFallbackFuncType.Exists)]
		[TestCase(TestFallbackFuncType.CrossSync)]
		public void Should_GetFallbackAction_Return_Preset_Action(TestFallbackFuncType testFallbackFuncType)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider();
			Action<CancellationToken> resultAction = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultAction = provider.GetFallbackAction();
					Assert.That(resultAction, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.Fallback = (_) => i++;
					resultAction =  provider.GetFallbackAction();
					resultAction(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.CrossSync:
					provider.FallbackAsync = async(_) => { await Task.Delay(1); i++; };
					resultAction = provider.GetFallbackAction();
					resultAction(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		[Test]
		[TestCase(TestFallbackFuncType.NoFuncs)]
		[TestCase(TestFallbackFuncType.Exists)]
		[TestCase(TestFallbackFuncType.CrossSync)]
		public async Task Should_GetAsyncFallbackFunc_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider();
			Func<CancellationToken, Task> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetAsyncFallbackFunc();
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.FallbackAsync = async(_) => { await Task.Delay(1); i++; };
					resultFunc = provider.GetAsyncFallbackFunc();
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.CrossSync:
					provider.Fallback = (_) => i++;
					resultFunc = provider.GetAsyncFallbackFunc();
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		[TestCase(TestFallbackFuncType.NoFuncs, null)]
		[TestCase(TestFallbackFuncType.Exists, null)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false)]
		public void Should_GetFallbackFunc_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType, bool? crossSync)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider();
			Func<CancellationToken, int> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetFallbackFunc<int>();
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.SetFallbackFunc((_) => i++);
					resultFunc = provider.GetFallbackFunc<int>();
					resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.FromNonGeneric:
					if (crossSync.Value)
					{
						provider.FallbackAsync = async (_) => { await Task.Delay(1); i++; };
					}
					else
					{
						provider.Fallback = (_) => i++;
					}
					resultFunc = provider.GetFallbackFunc<int>();
					resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		[TestCase(TestFallbackFuncType.NoFuncs, null)]
		[TestCase(TestFallbackFuncType.Exists, null)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, true)]
		[TestCase(TestFallbackFuncType.FromNonGeneric, false)]
		public async Task Should_GetAsyncFallbackFuncT_Return_Preset_Func(TestFallbackFuncType testFallbackFuncType, bool? crossSync)
		{
			FallbackFuncsProvider provider = new FallbackFuncsProvider();
			Func<CancellationToken, Task<int>> resultFunc = null;
			int i = 0;
			switch (testFallbackFuncType)
			{
				case TestFallbackFuncType.NoFuncs:
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					Assert.That(resultFunc, Is.Not.Null);
					break;
				case TestFallbackFuncType.Exists:
					provider.SetAsyncFallbackFunc(async (_) => { await Task.Delay(1); i++; return 0; });
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
				case TestFallbackFuncType.FromNonGeneric:
					if (crossSync.Value)
					{
						provider.Fallback = (_) => i++;
					}
					else
					{
						provider.FallbackAsync = async (_) => { await Task.Delay(1); i++; };
					}
					resultFunc = provider.GetAsyncFallbackFunc<int>(false);
					await resultFunc(default);
					Assert.That(i, Is.EqualTo(1));
					break;
			}
		}

		internal enum TestFallbackFuncType
		{
			NoFuncs,
			Exists,
			CrossSync,
			FromNonGeneric
		}
	}
}
