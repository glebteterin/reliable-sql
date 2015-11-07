using System;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sql
{
	public class AzureSqlStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			var msStrategy = new SqlDatabaseTransientErrorDetectionStrategy();

			var isTransient = msStrategy.IsTransient(ex);

			if (!isTransient)
			{
				SqlException sqlException;
				if ((sqlException = ex as SqlException) != null)
				{
					var msg = sqlException.ToString().ToLower();

					if (msg.Contains("physical connection is not usable"))
					{
						isTransient = true;
					}
					else if (msg.Contains("timeout expired"))
					{
						isTransient = true;
					}
				}
			}

			return isTransient;
		}
	}
}
