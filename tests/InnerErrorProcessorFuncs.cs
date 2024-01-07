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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public void ActionWithToken<TException>(TException _, CancellationToken __) => J++;

		public async Task AsyncFunc<TException>(TException _) { await Task.Delay(1); I++; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public async Task AsyncFuncWithToken<TException>(TException _, CancellationToken __) { await Task.Delay(1); J++; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public void ActionWithErrorInfo<TException>(TException _, ProcessingErrorInfo __) => K++;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public void ActionWithErrorInfoWithToken<TException>(TException _, ProcessingErrorInfo __, CancellationToken ___) => L++;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public async Task AsyncFuncWithErrorInfo<TException>(TException _, ProcessingErrorInfo __) { await Task.Delay(1); K++; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public async Task AsyncFuncWithErrorInfoWithToken<TException>(TException _, ProcessingErrorInfo __, CancellationToken ___) { await Task.Delay(1); L++; }
	}
}
