using System;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
//	public static class SaveExecutor
//	{
//		public static void Execute(Action<IDbConnection> action, RetryPolicy originalPolicy)
//		{
//			var attempts = 0;
//			var firstOccurrence = default(DateTime?);
//
//			EventHandler<RetryingEventArgs> handler = (sender, args) =>
//			{
//				attempts = args.CurrentRetryCount;
//				firstOccurrence = DateTime.UtcNow;
//			};
//
//			var policy = originalPolicy.Clone();
//
//			policy.Retrying += handler;
//
//			try
//			{
//				using (var cnn = CreateConnection())
//				{
//					action(cnn);
//				}
//			}
//			catch (Exception ex)
//			{
//				ex.AddRetryDetails(attempts, firstOccurrence.GetValueOrDefault(DateTime.UtcNow));
//
//				throw;
//			}
//			finally
//			{
//				_globalRetryPolicy.Retrying -= handler;
//				_globalRetryPolicy.Retrying -= GlobalConnectionPolicyOnRetrying;
//			}
//		}
//	}
}
