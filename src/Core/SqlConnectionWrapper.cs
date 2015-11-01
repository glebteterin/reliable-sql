using System;
using System.Data;
using System.Data.SqlClient;

namespace Sql
{
	public class SqlConnectionWrapper : IDbConnection
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

		#region IDbConnection implementation

		public string ConnectionString
		{
			get
			{
				return _connection.ConnectionString;
			}
			set
			{
				_connection.ConnectionString = value;
			}
		}

		public int ConnectionTimeout
		{
			get { return _connection.ConnectionTimeout; }
		}

		public string Database
		{
			get { return _connection.Database; }
		}

		public ConnectionState State
		{
			get { return _connection.State; }
		}

		public IDbTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		public IDbTransaction BeginTransaction(IsolationLevel iso)
		{
			return _connection.BeginTransaction(iso);
		}

		public void Close()
		{
			_connection.Close();
		}

		public void ChangeDatabase(string databaseName)
		{
			_connection.ChangeDatabase(databaseName);
		}

		public IDbCommand CreateCommand()
		{
			return new SqlCommandWrapper(this, _connection.CreateCommand(), _delay, _maxRetries);
		}

		public void Open()
		{
			_connection.Open();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_connection.State == ConnectionState.Open)
				{
					_connection.Close();
				}

				_connection.Dispose();
			}
		}

		#endregion

		#endregion
	}
}
