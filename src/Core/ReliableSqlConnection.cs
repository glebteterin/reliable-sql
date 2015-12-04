using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
	/// <summary>
	/// Implementation of retry logic for SqlConnection.
	/// </summary>
	public class ReliableSqlConnection : IDbConnection
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		private readonly SqlConnection _connection;
		private readonly SmartRetryPolicy _retryPolicy;

		private string _connectionString;

		internal static readonly SmartRetryPolicy DefaultRetryPolicy = new SmartRetryPolicy(new AzureSqlStrategy(), new FixedInterval(Constants.DefaultMaxRetries, TimeSpan.FromMilliseconds(Constants.DefaultDelayMs)));

		public ReliableSqlConnection(string connectionString)
			: this(connectionString, DefaultRetryPolicy)
		{
		}

		public ReliableSqlConnection(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryStrategy))
		{
		}

		public ReliableSqlConnection(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount))
		{
		}

		public ReliableSqlConnection(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, retryInterval))
		{
		}

		public ReliableSqlConnection(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, minBackoff, maxBackoff, deltaBackoff))
		{
		}

		public ReliableSqlConnection(string connectionString, ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: this(connectionString, new SmartRetryPolicy(errorDetectionStrategy, retryCount, initialInterval, increment))
		{
		}

		internal ReliableSqlConnection(string connectionString, SmartRetryPolicy retryPolicy)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			Debug.Assert(retryPolicy != null);

			_connectionString = connectionString;
			_retryPolicy = retryPolicy;

			_connection = new SqlConnection(_connectionString);
		}

		#region IDbConnection implementation

		/// <summary>
		/// Gets or sets the string used to open a SQL Server database.
		/// </summary>
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

		/// <summary>
		/// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
		/// </summary>
		public int ConnectionTimeout
		{
			get { return _connection.ConnectionTimeout; }
		}

		/// <summary>
		/// Gets the name of the current database or the database to be used after a connection is opened.
		/// </summary>
		public string Database
		{
			get { return _connection.Database; }
		}

		/// <summary>
		/// Indicates the state of the <see cref="T:GlebTeterin.ReliableSql.ReliableSqlConnection"/> during the most recent network operation performed on the connection.
		/// </summary>
		public ConnectionState State
		{
			get { return _connection.State; }
		}

		/// <summary>
		/// Starts a database transaction.
		/// </summary>
		public IDbTransaction BeginTransaction()
		{
			return _connection.BeginTransaction();
		}

		/// <summary>
		/// Starts a database transaction with the specified isolation level.
		/// </summary>
		public IDbTransaction BeginTransaction(IsolationLevel iso)
		{
			return _connection.BeginTransaction(iso);
		}

		/// <summary>
		/// Closes the connection to the database. This is the preferred method of closing any open connection.
		/// </summary>
		public void Close()
		{
			_connection.Close();
		}

		/// <summary>
		/// Changes the current database for an open <see cref="T:GlebTeterin.ReliableSql.ReliableSqlCommand"/>.
		/// </summary>
		public void ChangeDatabase(string databaseName)
		{
			_connection.ChangeDatabase(databaseName);
		}

		/// <summary>
		/// Creates and returns a <see cref="T:GlebTeterin.ReliableSql.ReliableSqlCommand"/> object associated with the <see cref="T:GlebTeterin.ReliableSql.ReliableSqlConnection"/>.
		/// </summary>
		public IDbCommand CreateCommand()
		{
			return new ReliableSqlCommand(this, _connection.CreateCommand(), _retryPolicy.Clone());
		}

		/// <summary>
		/// Opens a database connection with the property settings specified by the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlConnection.ConnectionString"/>.
		/// </summary>
		public void Open()
		{
			_retryPolicy.Clone().ExecuteAction(() =>
			{
				if (_connection.State != ConnectionState.Open)
				{
					Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlConnection: opening connection");

					_connection.Open();

					Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlConnection: connection opened");
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
