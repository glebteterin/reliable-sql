using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Sql;

namespace IntegrationTests
{
	public class TestResult
	{
		public Exception ThrownException { get; set; }

		public string Result { get; set; }

		public TestResult()
		{
		}

		public TestResult(Exception thrownException, string result)
		{
			ThrownException = thrownException;
			Result = result;
		}
	}

	public class TestExecutor
	{
		private readonly SqlConnectionWrapper _wrapper;

		public TestExecutor(string connectionString, TimeSpan delay, int maxRetries)
		{
			_wrapper = new SqlConnectionWrapper(new SqlConnection(connectionString), delay, maxRetries);
		}

		public TestResult Execute(Guid id, int attempts, string errorType)
		{
			var catchedException = default(Exception);
			var returnedValue = "";

			var param = new
			{
				id = id,
				errorRepeat = attempts,
				errorType = errorType
			};

			try
			{
				returnedValue = _wrapper
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

			return new TestResult(catchedException, returnedValue);
		}
	}
}
