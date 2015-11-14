using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
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

		public string CommandText
		{
			get { return _sqlCommandToWrap.CommandText; }
			set { _sqlCommandToWrap.CommandText = value; }
		}

		public int CommandTimeout
		{
			get { return _sqlCommandToWrap.CommandTimeout; }
			set { _sqlCommandToWrap.CommandTimeout = value; }
		}

		public CommandType CommandType
		{
			get { return _sqlCommandToWrap.CommandType; }
			set { _sqlCommandToWrap.CommandType = value; }
		}

		public IDataParameterCollection Parameters
		{
			get { return _sqlCommandToWrap.Parameters; }
		}

		public UpdateRowSource UpdatedRowSource
		{
			get { return _sqlCommandToWrap.UpdatedRowSource; }
			set { _sqlCommandToWrap.UpdatedRowSource = value; }
		}

		public void Dispose()
		{
			_sqlCommandToWrap.Dispose();
		}

		public void Prepare()
		{
			_sqlCommandToWrap.Prepare();
		}

		public void Cancel()
		{
			_sqlCommandToWrap.Cancel();
		}

		public IDbDataParameter CreateParameter()
		{
			return _sqlCommandToWrap.CreateParameter();
		}

		#endregion
	}
}
