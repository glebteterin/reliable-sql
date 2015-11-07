using System;
using System.Data;

namespace Sql
{
	public class ConnectionManager
	{
		private const int DefaultMaxRetries = 10;
		private static readonly TimeSpan DefaultDelay = new TimeSpan(0, 0, 0, 0, 100);

		private readonly string _connectionString;
		private readonly TimeSpan _delay;
		private readonly int _maxRetries;

		public ConnectionManager(string connectionString)
			: this(connectionString, DefaultDelay, DefaultMaxRetries)
		{
		}

		public ConnectionManager(string connectionString, TimeSpan delay, int maxRetries)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_connectionString = connectionString;
			_delay = delay;
			_maxRetries = maxRetries;
		}

		public virtual SqlConnectionWrapper CreateConnection()
		{
			return new SqlConnectionWrapper(
				_connectionString,
				_delay,
				_maxRetries);
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
