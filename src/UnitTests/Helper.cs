using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace UnitTests
{
	public class Helper
	{
		public static IEnumerable<SqlException> GenerateExceptions(IEnumerable<KeyValuePair<int, string>> errorCodes)
		{
			var errors =
				errorCodes
					.Select(errorCode =>
						new SqlExceptionBuilder()
							.WithErrorNumber(errorCode.Key)
							.WithErrorMessage(errorCode.Value)
							.BuildComplete())
					.ToList();

			return errors;
		}
	}
}
