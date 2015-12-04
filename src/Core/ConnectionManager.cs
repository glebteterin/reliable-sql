using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
	/// <summary>
	/// <see cref="P:GlebTeterin.ReliableSql.ReliableSqlConnection"/> factory.
	/// </summary>
	public class ConnectionManager
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		private readonly string _connectionString;
		private readonly SmartRetryPolicy _globalRetryPolicy;

		/// <summary>
		/// Occurs when a retry condition is encountered.
		/// </summary>
		public event EventHandler<RetryingEventArgs> Retrying;

		protected ConnectionManager(string connectionString, SmartRetryPolicy retryPolicy)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");
			if (retryPolicy == null) throw new ArgumentNullException("retryPolicy");

			_connectionString = connectionString;
			_globalRetryPolicy = retryPolicy;
			_globalRetryPolicy.Subscribe(GlobalConnectionPolicyOnRetrying);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GlebTeterin.ReliableSql.ConnectionManager"/> class with a connection string and default instance of <see cref="P:GlebTeterin.ReliableSql.AzureSqlStrategy"/>.
		/// </summary>
		public ConnectionManager(string connectionString)
			: this(connectionString, ReliableSqlConnection.DefaultRetryPolicy)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GlebTeterin.ReliableSql.ConnectionManager"/> class with a connection string and a <see cref="P:GlebTeterin.ReliableSql.AzureSqlStrategy"/>.
		/// </summary>
		public ConnectionManager(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryStrategy))
		{
		}

		public ConnectionManager(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount))
		{
		}

		public ConnectionManager(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, retryInterval))
		{
		}

		public ConnectionManager(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, minBackoff, maxBackoff, deltaBackoff))
		{
		}

		public ConnectionManager(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, initialInterval, increment))
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="P:GlebTeterin.ReliableSql.ReliableSqlConnection"/>.
		/// </summary>
		public virtual ReliableSqlConnection CreateConnection()
		{
			Tracer.TraceEvent(TraceEventType.Verbose, 0, "ConnectionManager: Creating connection");

			return new ReliableSqlConnection(
				_connectionString,
				_globalRetryPolicy.Clone());
		}

		/// <summary>
		/// Provides a safe way of using <see cref="P:GlebTeterin.ReliableSql.ReliableSqlConnection"/>.
		/// </summary>
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
