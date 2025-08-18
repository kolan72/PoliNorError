using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Extensions.PolicyResultHandling
{
	public static class PolicyResultHandling
	{
		/// <summary>
		/// Adds a synchronous handler for a policy result without cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the policy result.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds a synchronous handler for a policy result with cancellation support based on the specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the policy result.</param>
		/// <param name="convertType">The type of cancellation to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddHandlerForPolicyResult(action.ToCancelableAction(convertType));
		}

		/// <summary>
		/// Adds a synchronous handler for a policy result with cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the policy result, accepting a cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a policy result without cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the policy result.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a policy result with cancellation support based on the specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the policy result.</param>
		/// <param name="convertType">The type of cancellation to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddHandlerForPolicyResult(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a policy result with cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the policy result, accepting a cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a typed policy result without cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the typed policy result.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a typed policy result with cancellation support based on the specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the typed policy result.</param>
		/// <param name="convertType">The type of cancellation to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddHandlerForPolicyResult(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds an asynchronous handler for a typed policy result with cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="func">The asynchronous function to execute when handling the typed policy result, accepting a cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds a synchronous handler for a typed policy result without cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the typed policy result.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		/// <summary>
		/// Adds a synchronous handler for a typed policy result with cancellation support based on the specified cancellation type.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the typed policy result.</param>
		/// <param name="convertType">The type of cancellation to apply.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddHandlerForPolicyResult(action.ToCancelableAction(convertType));
		}

		/// <summary>
		/// Adds a synchronous handler for a typed policy result with cancellation support.
		/// </summary>
		/// <typeparam name="T">The type of the policy, which must inherit from <see cref="Policy"/>.</typeparam>
		/// <typeparam name="U">The type of the policy result data.</typeparam>
		/// <param name="errorPolicyBase">The policy to which the handler is added.</param>
		/// <param name="action">The action to execute when handling the typed policy result, accepting a cancellation token.</param>
		/// <returns>The policy instance for method chaining.</returns>
		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}
	}
}
