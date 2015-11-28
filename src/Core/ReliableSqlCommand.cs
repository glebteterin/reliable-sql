using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
	/// <summary>
	/// Implementation of retry logic for SqlCommand.
	/// </summary>
	public class ReliableSqlCommand : IDbCommand
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		private readonly SqlCommand _sqlCommandToWrap;
		private readonly RetryPolicy _retryPolicy;

		private ReliableSqlConnection _currentConnection;

		internal ReliableSqlCommand(ReliableSqlConnection currentConnection, SqlCommand sqlCommandToWrap, RetryPolicy retryPolicy)
		{
			Debug.Assert(currentConnection != null, "currentConnection param is null");
			Debug.Assert(sqlCommandToWrap != null, "sqlCommandToWrap param is null");
			Debug.Assert(retryPolicy != null, "retryPolicy param is null");

			_currentConnection = currentConnection;
			_sqlCommandToWrap = sqlCommandToWrap;
			_retryPolicy = retryPolicy;
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		public int ExecuteNonQuery()
		{
			Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlCommand => ExecuteNonQuery");

			return _retryPolicy.ExecuteAction(() =>
			{
				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteNonQuery();
			});
		}

		/// <summary>
		/// Sends the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlCommand.CommandText"/> to the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlCommand.Connection"/> and builds a <see cref="T:System.Data.SqlClient.SqlDataReader"/>.
		/// </summary>
		public IDataReader ExecuteReader()
		{
			return _retryPolicy.ExecuteAction(() =>
			{
				Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlCommand => ExecuteReader");

				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteReader();
			});
		}

		/// <summary>
		/// Sends the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlCommand.CommandText"/> to the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlCommand.Connection"/>, and builds a <see cref="T:System.Data.SqlClient.SqlDataReader"/> using one of the <see cref="T:System.Data.CommandBehavior"/> values.
		/// </summary>
		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return _retryPolicy.ExecuteAction(() =>
			{
				Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlCommand => ExecuteReader {0}", behavior);

				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteReader(behavior);
			});
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		public object ExecuteScalar()
		{
			return _retryPolicy.ExecuteAction(() =>
			{
				Tracer.TraceEvent(TraceEventType.Verbose, 0, "ReliableSqlCommand => ExecuteScalar");

				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteScalar();
			});
		}

		#region IDbCommand proxy implementation

		/// <summary>
		/// Gets or sets the <see cref="T:GlebTeterin.ReliableSql.ReliableSqlConnection"/> used by this instance of the <see cref="T:GlebTeterin.ReliableSql.ReliableSqlCommand"/>.
		/// </summary>
		public IDbConnection Connection
		{
			get { return _currentConnection; }
			set
			{
				ReliableSqlConnection cnn = null;

				if (value == null)
					throw new ArgumentNullException();

				if ((cnn = (value as ReliableSqlConnection)) == null)
					throw new ArgumentException(string.Format("Unsupported connection type ({0})",
						value.GetType().Name));

				_currentConnection = cnn;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:System.Data.SqlClient.SqlTransaction"/> within which the <see cref="T:GlebTeterin.ReliableSql.ReliableSqlCommand"/> executes.
		/// </summary>
		public IDbTransaction Transaction
		{
			get
			{
				return _sqlCommandToWrap.Transaction;
			}
			set
			{
				SqlTransaction tran = null;

				if (value == null)
				{
					_sqlCommandToWrap.Transaction = null;
					return;
				}

				if ((tran = (value as SqlTransaction)) == null)
					throw new ArgumentException(string.Format("Unsupported transaction type ({0})",
						value.GetType().Name));

				_sqlCommandToWrap.Transaction = tran;
			}
		}

		/// <summary>
		/// Gets or sets the Transact-SQL statement, table name or stored procedure to execute at the data source.
		/// </summary>
		public string CommandText
		{
			get { return _sqlCommandToWrap.CommandText; }
			set { _sqlCommandToWrap.CommandText = value; }
		}

		/// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// </summary>
		public int CommandTimeout
		{
			get { return _sqlCommandToWrap.CommandTimeout; }
			set { _sqlCommandToWrap.CommandTimeout = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating how the <see cref="P:GlebTeterin.ReliableSql.ReliableSqlCommand.CommandText"/> property is to be interpreted.
		/// </summary>
		public CommandType CommandType
		{
			get { return _sqlCommandToWrap.CommandType; }
			set { _sqlCommandToWrap.CommandType = value; }
		}

		/// <summary>
		/// Gets the <see cref="T:System.Data.SqlClient.SqlParameterCollection"/>.
		/// </summary>
		public IDataParameterCollection Parameters
		{
			get { return _sqlCommandToWrap.Parameters; }
		}

		/// <summary>
		/// Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow"/> when used by the Update method of the <see cref="T:System.Data.Common.DbDataAdapter"/>.
		/// </summary>
		public UpdateRowSource UpdatedRowSource
		{
			get { return _sqlCommandToWrap.UpdatedRowSource; }
			set { _sqlCommandToWrap.UpdatedRowSource = value; }
		}

		public void Dispose()
		{
			_sqlCommandToWrap.Dispose();
		}

		/// <summary>
		/// Creates a prepared version of the command on an instance of SQL Server.
		/// </summary>
		public void Prepare()
		{
			_sqlCommandToWrap.Prepare();
		}

		/// <summary>
		/// Tries to cancel the execution of a <see cref="T:GlebTeterin.ReliableSql.ReliableSqlCommand"/>.
		/// </summary>
		public void Cancel()
		{
			_sqlCommandToWrap.Cancel();
		}

		/// <summary>
		/// Creates a new instance of a <see cref="T:System.Data.SqlClient.SqlParameter"/> object.
		/// </summary>
		public IDbDataParameter CreateParameter()
		{
			return _sqlCommandToWrap.CreateParameter();
		}

		#endregion
	}
}
