using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
	public class SmartRetryPolicy : RetryPolicy
	{
		private EventHandler<RetryingEventArgs> _retryingHandler;

		public SmartRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
			: base(errorDetectionStrategy, retryStrategy)
		{
		}

		public SmartRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryStrategy)
			: base(errorDetectionStrategy, retryStrategy)
		{
		}

		public SmartRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryStrategy, TimeSpan retryInterval)
			: base(errorDetectionStrategy, retryStrategy, retryInterval)
		{
		}

		public SmartRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryStrategy, TimeSpan retryInterval, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: base(errorDetectionStrategy, retryStrategy, retryInterval, maxBackoff, deltaBackoff)
		{
		}

		public SmartRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryStrategy, TimeSpan initialInterval, TimeSpan maxBackoff)
			: base(errorDetectionStrategy, retryStrategy, initialInterval, maxBackoff)
		{
		}

		public override TResult ExecuteAction<TResult>(Func<TResult> func)
		{
			var result = default(TResult);

			var attempts = 0;
			var firstOccurrence = default(DateTime?);

			EventHandler<RetryingEventArgs> handler = (sender, args) =>
			{
				attempts = args.CurrentRetryCount;
				firstOccurrence = DateTime.UtcNow;
			};

			Retrying += handler;

			try
			{
				result = base.ExecuteAction(func);
			}
			catch (Exception ex)
			{
				ex.AddRetryDetails(attempts, firstOccurrence.GetValueOrDefault(DateTime.UtcNow));

				throw;
			}
			finally
			{
				Retrying -= handler;
			}

			return result;
		}

		public void Subscribe(EventHandler<RetryingEventArgs> retryingHandler)
		{
			_retryingHandler = retryingHandler;

			this.Retrying += retryingHandler;
		}

		public SmartRetryPolicy Clone()
		{
			var cloned = new SmartRetryPolicy(ErrorDetectionStrategy, RetryStrategy);

			if (_retryingHandler != null)
				cloned.Subscribe(_retryingHandler);

			return cloned;
		}
	}
}
