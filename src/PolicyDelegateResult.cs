﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace PoliNorError
{
	public sealed class PolicyDelegateResult : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult result, string policyName, MethodInfo methodInfo) : base(result, policyName, methodInfo)
		{
			Result = result;
		}

		public PolicyResult Result { get; }

		internal override PolicyResult GetResult() => Result;
	}

	public sealed class PolicyDelegateResult<T> : PolicyDelegateResultBase
	{
		internal PolicyDelegateResult(PolicyResult<T> result, string policyName, MethodInfo methodInfo) : base(result, policyName, methodInfo)
		{
			Result = result;
		}

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

		public string PolicyName { get; }

		public MethodInfo PolicyMethodInfo { get; }

		public bool IsFailed { get; }

		public bool IsSuccess { get; }

		public bool IsCanceled { get; }

		public PolicyResultFailedReason FailedReason { get; internal set; }

		public IEnumerable<Exception> Errors { get; }

		internal abstract PolicyResult GetResult();
	}
}
