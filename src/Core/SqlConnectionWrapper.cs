using System;
using System.Data;
using System.Data.SqlClient;

namespace Sql
{
	public class SqlConnectionWrapper
	{
		private const int DefaultMaxRetries = 10;
		private static readonly TimeSpan DefaultDelay = new TimeSpan(0,0,0,0,100);

		private readonly SqlConnection _connection;
		private readonly int _maxRetries;
		private readonly TimeSpan _delay;

		public SqlConnectionWrapper(SqlConnection connection)
			: this(connection, DefaultDelay, DefaultMaxRetries)
		{
			
		}

		public SqlConnectionWrapper(SqlConnection connection, TimeSpan delay, int maxRetries)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
			_delay = delay;
			_maxRetries = maxRetries;
		}

		public virtual void Execute(Action<IDbConnection> action)
		{
			using (_connection)
			{
				SimpleRetry.Do(() => action(_connection), _delay, _maxRetries);
			}
		}
	}
}
