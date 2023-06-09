﻿using System;

namespace PoliNorError
{
	internal enum SettingPolicyDelegateResult
	{
		None = 0,
		Success,
		Empty,
		AlreadySet
	}

	internal static class SettingPolicyDelegateResultExtensions
	{
		public static void ThrowErrorByResult(this SettingPolicyDelegateResult res)
		{
			switch (res)
			{
				case SettingPolicyDelegateResult.Empty:
					throw new InvalidOperationException("Policy collection is empty.");
				case SettingPolicyDelegateResult.AlreadySet:
					throw new InvalidOperationException("Policy collection contains at least one policy delegate.");
				default:
					throw new NotImplementedException();
			}
		}
	}
}
