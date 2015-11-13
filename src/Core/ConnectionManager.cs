using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sql
{
	public class ConnectionManager
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		private const int DefaultMaxRetries = 10;
		private const int DefaultDelayMs = 100;

		private readonly string _connectionString;
		private readonly RetryPolicy _globalRetryPolicy;

		public event EventHandler<RetryingEventArgs> Retrying;

		private static RetryPolicy DefaultRetryPolicy
		{
			get
			{
				return new RetryPolicy(new AzureSqlStrategy(), DefaultMaxRetries, TimeSpan.FromMilliseconds(DefaultDelayMs));
			}
		}

		public ConnectionManager(string connectionString)
			: this(connectionString, DefaultRetryPolicy)
		{
		}

		public ConnectionManager(string connectionString, RetryPolicy retryPolicy)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_connectionString = connectionString;
			_globalRetryPolicy = retryPolicy;
			_globalRetryPolicy.Retrying += GlobalConnectionPolicyOnRetrying;
		}

		public virtual SqlConnectionWrapper CreateConnection()
		{
			Tracer.TraceEvent(TraceEventType.Verbose, 0, "ConnectionManager: Creating connection");

			return new SqlConnectionWrapper(
				_connectionString,
				_globalRetryPolicy);
		}

		public virtual void Execute(Action<IDbConnection> action)
		{
			using (var cnn = CreateConnection())
			{
				action(cnn);
			}
		}

		private void GlobalConnectionPolicyOnRetrying(object sender, RetryingEventArgs retryingEventArgs)
		{
			if (Retrying != null)
			{
				var ev = Retrying;
				ev(sender, retryingEventArgs);
			}
		}
	}
}
