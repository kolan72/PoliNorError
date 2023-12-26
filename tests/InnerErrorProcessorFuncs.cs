using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class InnerErrorProcessorFuncs
	{
		public int I { get; private set; }

		public int J { get; private set; }

		public int K { get; private set; }

		public int L { get; private set; }

		public void Action<TException>(TException _) => I++;

		public void ActionWithToken<TException>(TException _, CancellationToken __) => J++;

		public async Task AsyncFunc<TException>(TException _) { await Task.Delay(1); I++; }

		public async Task AsyncFuncWithToken<TException>(TException _, CancellationToken __) { await Task.Delay(1); J++; }

		public void ActionWithErrorInfo<TException>(TException _, ProcessingErrorInfo __) => K++;

		public void ActionWithErrorInfoWithToken<TException>(TException _, ProcessingErrorInfo __, CancellationToken ___) => L++;

		public async Task AsyncFuncWithErrorInfo<TException>(TException _, ProcessingErrorInfo __) { await Task.Delay(1); K++; }

		public async Task AsyncFuncWithErrorInfoWithToken<TException>(TException _, ProcessingErrorInfo __, CancellationToken ___) { await Task.Delay(1); L++; }
	}
}
