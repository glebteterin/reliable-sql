using System;
using System.Data;
using System.Data.SqlClient;

namespace Sql
{
	public class SqlConnectionWrapper : IDbConnection
	{
		private readonly SqlConnection _connection;
		private readonly int _maxRetries;
		private readonly TimeSpan _delay;

		private string _connectionString;

		public SqlConnectionWrapper(string connectionString, TimeSpan delay, int maxRetries)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_connectionString = connectionString;
			_delay = delay;
			_maxRetries = maxRetries;

			_connection = new SqlConnection(_connectionString);
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
				return _connectionString;
			}
			set
			{
				_connectionString = value;
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
			SimpleRetry.Do(() => _connection.Open(), _delay, _maxRetries);
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
