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

		private const int DefaultMaxRetries = 10;
		private const int DefaultDelayMs = 100;

		private readonly string _connectionString;
		private readonly RetryPolicy _globalRetryPolicy;

		/// <summary>
		/// Occurs when a retry condition is encountered.
		/// </summary>
		public event EventHandler<RetryingEventArgs> Retrying;

		private static RetryPolicy DefaultRetryPolicy
		{
			get
			{
				return new RetryPolicy(new AzureSqlStrategy(), DefaultMaxRetries, TimeSpan.FromMilliseconds(DefaultDelayMs));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GlebTeterin.ReliableSql.ConnectionManager"/> class with a connection string and default instance of <see cref="P:GlebTeterin.ReliableSql.AzureSqlStrategy"/>.
		/// </summary>
		public ConnectionManager(string connectionString)
			: this(connectionString, DefaultRetryPolicy)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GlebTeterin.ReliableSql.ConnectionManager"/> class with a connection string and a <see cref="P:GlebTeterin.ReliableSql.AzureSqlStrategy"/>.
		/// </summary>
		public ConnectionManager(string connectionString, RetryPolicy retryPolicy)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_connectionString = connectionString;
			_globalRetryPolicy = retryPolicy;
			_globalRetryPolicy.Retrying += GlobalConnectionPolicyOnRetrying;
		}

		/// <summary>
		/// Creates a new instance of <see cref="P:GlebTeterin.ReliableSql.ReliableSqlConnection"/>.
		/// </summary>
		public virtual ReliableSqlConnection CreateConnection()
		{
			Tracer.TraceEvent(TraceEventType.Verbose, 0, "ConnectionManager: Creating connection");

			return new ReliableSqlConnection(
				_connectionString,
				_globalRetryPolicy);
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
