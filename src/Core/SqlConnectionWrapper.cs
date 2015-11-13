using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sql
{
	public class SqlConnectionWrapper : IDbConnection
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		private readonly SqlConnection _connection;
		private readonly RetryPolicy _retryPolicy;

		private string _connectionString;

		public SqlConnectionWrapper(string connectionString, RetryPolicy retryPolicy)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");
			if (retryPolicy == null) throw new ArgumentNullException("retryPolicy");

			_connectionString = connectionString;
			_retryPolicy = retryPolicy;

			_connection = new SqlConnection(_connectionString);
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
			return new SqlCommandWrapper(this, _connection.CreateCommand(), _retryPolicy);
		}

		public void Open()
		{
			_retryPolicy.ExecuteAction(() =>
			{
				if (_connection.State != ConnectionState.Open)
				{
					Tracer.TraceEvent(TraceEventType.Verbose, 0, "SqlConnectionWrapper: opening connection");

					_connection.Open();

					Tracer.TraceEvent(TraceEventType.Verbose, 0, "SqlConnectionWrapper: connection opened");
				}
			});
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
