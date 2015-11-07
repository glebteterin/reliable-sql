using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sql
{
	public class SqlCommandWrapper : IDbCommand
	{
		private readonly SqlCommand _sqlCommandToWrap;
		private readonly RetryPolicy _retryPolicy;

		private SqlConnectionWrapper _currentConnection;

		public SqlCommandWrapper(SqlConnectionWrapper currentConnection, SqlCommand sqlCommandToWrap, RetryPolicy retryPolicy)
		{
			_currentConnection = currentConnection;
			_sqlCommandToWrap = sqlCommandToWrap;
			_retryPolicy = retryPolicy;
		}

		public int ExecuteNonQuery()
		{
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
				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteReader();
			});
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return _retryPolicy.ExecuteAction(() =>
			{
				if (Connection.State != ConnectionState.Open)
					Connection.Open();

				return _sqlCommandToWrap.ExecuteReader(behavior);
			});
		}

		public object ExecuteScalar()
		{
			return _retryPolicy.ExecuteAction(() =>
			{
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
				SqlConnectionWrapper cnn = null;

				if (value == null)
					throw new ArgumentNullException();

				if ((cnn = (value as SqlConnectionWrapper)) == null)
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
