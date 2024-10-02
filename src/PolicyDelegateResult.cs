using System;
using System.Collections.Generic;
using System.Reflection;

namespace PoliNorError
{
	/// <summary>
	/// Contains the <see cref="PolicyResult"/> result of delegate handling by a Policy or <see cref="PolicyDelegate"></see>
	/// that additionally contains the delegate's <see cref="MethodInfo"></see>  in the <see cref="PolicyDelegateResultBase.PolicyMethodInfo"/> property.
	/// </summary>
	public sealed class PolicyDelegateResult : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult result, string policyName, MethodInfo methodInfo) : base(result, policyName, methodInfo)
		{
			Result = result;
		}

		///<inheritdoc cref = "PolicyResult"/>
		public PolicyResult Result { get; }

		internal override PolicyResult GetResult() => Result;
	}

	/// <summary>
	/// Contains the <see cref="PolicyResult{T}"/> result of delegate handling by a Policy or <see cref="PolicyDelegate{T}"></see>
	/// that additionally contains the delegate's <see cref="MethodInfo"></see>  in the <see cref="PolicyDelegateResultBase.PolicyMethodInfo"/> property.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	public sealed class PolicyDelegateResult<T> : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult<T> result, string policyName, MethodInfo methodInfo) : base(result, policyName, methodInfo)
		{
			Result = result;
		}

		///<inheritdoc cref = "PolicyResult{T}"/>
		public PolicyResult<T> Result { get; }

		internal override PolicyResult GetResult() => Result;
	}

	public abstract class PolicyDelegateResultBase
	{
		protected PolicyDelegateResultBase(PolicyResult policyResult, string policyName, MethodInfo methodInfo)
		{
			PolicyName = policyName;
			PolicyMethodInfo = methodInfo;
			IsFailed = policyResult.IsFailed;
			IsSuccess = policyResult.IsSuccess;
			IsCanceled = policyResult.IsCanceled;
			FailedReason = policyResult.FailedReason;
			Errors = policyResult.Errors;
		}

		/// <summary>
		/// Represents the name of the policy whose results this PolicyDelegateResult contains.
		/// </summary>
		public string PolicyName { get; }

		/// <summary>
		/// <see cref="MethodInfo"/> of the handling delegate.
		/// </summary>
		public MethodInfo PolicyMethodInfo { get; }

		///<inheritdoc cref = "PolicyResult.IsFailed"/>
		public bool IsFailed { get; }

		///<inheritdoc cref = "PolicyResult.IsSuccess"/>
		public bool IsSuccess { get; }

		///<inheritdoc cref = "PolicyResult.IsCanceled"/>
		public bool IsCanceled { get; }

		///<inheritdoc cref = "PolicyResult.FailedReason"/>
		public PolicyResultFailedReason FailedReason { get; internal set; }

		///<inheritdoc cref = "PolicyResult.Errors"/>
		public IEnumerable<Exception> Errors { get; }

		internal abstract PolicyResult GetResult();
	}
}
