using System;
using System.Data;
using System.Data.SqlClient;

namespace Sql
{
	public class SqlCommandWrapper : IDbCommand
	{
		private readonly SqlCommand _sqlCommandToWrap;

		private readonly TimeSpan _delay;
		private readonly int _maxRetries;

		private SqlConnectionWrapper _currentConnection;

		public SqlCommandWrapper(SqlConnectionWrapper currentConnection, SqlCommand sqlCommandToWrap, TimeSpan delay, int maxRetries)
		{
			_currentConnection = currentConnection;
			_sqlCommandToWrap = sqlCommandToWrap;
			_delay = delay;
			_maxRetries = maxRetries;
		}

		public int ExecuteNonQuery()
		{
			return SimpleRetry.Do(() => _sqlCommandToWrap.ExecuteNonQuery(), _delay, _maxRetries);
		}

		public IDataReader ExecuteReader()
		{
			return SimpleRetry.Do(() => _sqlCommandToWrap.ExecuteReader(), _delay, _maxRetries);
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return SimpleRetry.Do(() => _sqlCommandToWrap.ExecuteReader(behavior), _delay, _maxRetries);
		}

		public object ExecuteScalar()
		{
			return SimpleRetry.Do(() => _sqlCommandToWrap.ExecuteScalar(), _delay, _maxRetries);
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
