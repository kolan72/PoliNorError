using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class FuncExtensionsTests
	{
		[Test]
		public void Should_Simple_ToPrecancelableAction_Work()
		{
			int i = 0;
			Action act = () => i++;
			var cs = new CancellationTokenSource();
			cs.Cancel();
			act.ToPrecancelableAction()(cs.Token);
			ClassicAssert.AreEqual(0, i);

			var cs2 = new CancellationTokenSource();
			act.ToPrecancelableAction()(cs2.Token);
			ClassicAssert.AreEqual(1, i);
			cs.Dispose();
			cs2.Dispose();
		}

		[Test]
		public void Should_ToPrecancelableAction_WithParam_Work()
		{
			int i = 0;
			Action<Exception> act = (_) => i++;
			var cs = new CancellationTokenSource();
			cs.Cancel();

			var someException = new Exception();

			act.ToPrecancelableAction()(someException,  cs.Token);
			ClassicAssert.AreEqual(0, i);

			var cs2 = new CancellationTokenSource();
			act.ToPrecancelableAction()(someException, cs2.Token);
			ClassicAssert.AreEqual(1, i);
			cs.Dispose();
			cs2.Dispose();
		}

		[Test]
		public async Task Should_ToPrecancelableFunc_ForAsync_Work()
		{
			int i = 0;
			Func<Task> func = async () => { await Task.Delay(1); i++; };
			var cs = new CancellationTokenSource();
			cs.Cancel();
			await func.ToPrecancelableFunc()(cs.Token);
			ClassicAssert.AreEqual(0, i);

			var cs2 = new CancellationTokenSource();
			await func.ToPrecancelableFunc()(cs2.Token);
			ClassicAssert.AreEqual(1, i);
			cs.Dispose();
			cs2.Dispose();
		}
	}
}