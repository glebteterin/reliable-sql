using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace GlebTeterin.ReliableSql
{
	public class AzureSqlStrategy : ITransientErrorDetectionStrategy
	{
		private readonly static TraceSource Tracer = new TraceSource(Constants.TraceSourceName);

		public bool IsTransient(Exception ex)
		{
			Tracer.TraceEvent(TraceEventType.Verbose, 0, "AzureSqlStrategy is starting");

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
						Tracer.TraceEvent(TraceEventType.Verbose, 0, "AzureSqlStrategy: physical connection is not usable");

						isTransient = true;
					}
					else if (msg.Contains("timeout expired"))
					{
						Tracer.TraceEvent(TraceEventType.Verbose, 0, "AzureSqlStrategy: timeout expired");

						isTransient = true;
					}
				}
			}

			Tracer.TraceEvent(TraceEventType.Verbose, 0, "AzureSqlStrategy: is transient: {0}", isTransient);

			return isTransient;
		}
	}
}
