using System;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sql;

namespace IntegrationTests
{
	public class TestResult
	{
		public Exception ThrownException { get; set; }

		public int RetryCount { get; set; }

		public string Result { get; set; }

		public TestResult()
		{
		}

		public TestResult(Exception thrownException, int retryCount, string result)
		{
			ThrownException = thrownException;
			RetryCount = retryCount;
			Result = result;
		}
	}

	public class TestExecutor
	{
		private readonly ConnectionManager _connectionManager;

		public TestExecutor(string connectionString, RetryPolicy retryPolicy)
		{
			_connectionManager = new ConnectionManager(connectionString, retryPolicy);
		}

		public TestResult Execute(Guid id, int attempts, string errorType)
		{
			var catchedException = default(Exception);
			var returnedValue = "";
			var retryCount = 0;

			var param = new
			{
				id = id,
				errorRepeat = attempts,
				errorType = errorType
			};

			_connectionManager.Retrying += (sender, args) => ++retryCount;

			try
			{
				returnedValue = _connectionManager.CreateConnection()
									.Query<string>(
										"SqlWrapperTest",
										commandType: CommandType.StoredProcedure,
										param: param)
									.FirstOrDefault();
			}
			catch (Exception ex)
			{
				catchedException = ex;
			}

			return new TestResult(catchedException, retryCount, returnedValue);
		}
	}
}
