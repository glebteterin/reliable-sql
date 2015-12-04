using System;

namespace GlebTeterin.ReliableSql
{
	public static class Extensions
	{
		public static void AddRetryDetails(this Exception ex, int attempts, DateTime firstOccurrence)
		{
			ex.Data[Constants.ExceptionAttempts] = attempts;
			ex.Data[Constants.ExceptionFirstOccurrence] = firstOccurrence;
		}
	}
}
