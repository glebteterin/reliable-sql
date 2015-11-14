using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace IntegrationTests
{
	public class MockRetryStrategy : ITransientErrorDetectionStrategy
	{
		private readonly List<int> _transientErrors = new List<int>{50001, 50002};

		public bool IsTransient(Exception ex)
		{
			var isTransient = false;

			SqlException sqlException;
			if ((sqlException = ex as SqlException) != null)
			{
				var errorNumber = sqlException.Number;

				if (_transientErrors.Contains(errorNumber))
				{
					isTransient = true;
				}
			}

			return isTransient;
		}
	}
}
