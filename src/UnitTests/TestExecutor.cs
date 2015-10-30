using System;
using System.Data;
using Sql;

namespace UnitTests
{
	public class TestResult
	{
		public Exception ThrownException { get; set; }

		public object Result { get; set; }

		public int AttemptCount { get; set; }

		public TestResult()
		{
		}

		public TestResult(Exception thrownException, object result, int attemptCount)
		{
			ThrownException = thrownException;
			Result = result;
			AttemptCount = attemptCount;
		}
	}

	public class TestExecutor
	{
		private readonly SqlConnectionWrapper _wrapper;

		public TestExecutor(SqlConnectionWrapper wrapper)
		{
			_wrapper = wrapper;
		}

		public TestResult Execute(Func<IDbConnection, object> action)
		{
			var attemptCounter = 0;
			var catchedException = default(Exception);
			var returnedValue = default(object);

			try
			{
				_wrapper.Execute(cnn =>
				{
					++attemptCounter;

					returnedValue = action(cnn);
				});
			}
			catch (Exception ex)
			{
				catchedException = ex;
			}

			return new TestResult(catchedException, returnedValue, attemptCounter);
		}
	}
}
