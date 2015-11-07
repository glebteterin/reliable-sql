using System;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sql
{
	public class ConnectionManager
	{
		private const int DefaultMaxRetries = 10;
		private const int DefaultDelayMs = 100;

		private readonly string _connectionString;
		private readonly RetryPolicy _globalRetryPolicy;

		private static RetryPolicy DefaultRetryPolicy
		{
			get
			{
				return new RetryPolicy(new SqlDatabaseTransientErrorDetectionStrategy(), DefaultMaxRetries, TimeSpan.FromMilliseconds(DefaultDelayMs));
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
		}

		public virtual SqlConnectionWrapper CreateConnection()
		{
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
	}
}
