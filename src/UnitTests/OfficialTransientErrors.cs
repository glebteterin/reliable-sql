using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnitTests
{
	public static class OfficialTransientErrors
	{
		private static readonly Dictionary<int, string> TransientErrors = new Dictionary<int, string>
		{
			{40501, "The service is currently busy. Retry the request after 10 seconds. Incident ID: %ls. Code: %d"},
			{40540, "A severe error occurred on the current command. The results, if any, should be discarded."},
			{40613, "Database '%.*ls' on server '%.*ls' is not currently available.  Please retry the connection later.  If the problem persists, contact customer support, and provide them the session tracing ID of '%.*ls'."},
			{10928, "Resource ID: %d. The %s limit for the database is %d and has been reached."},
			{10929, "A transport-level error has occurred when receiving results from the server."},
			{40143, "The replica that the data node hosts for the requested partition is not primary."},
			{40197, "The service has encountered an error processing your request. Please try again. Error code %d."},
			{233, "The column '%.*ls' in table '%.*ls' cannot be null."},
			{10053, "Could not convert the data value due to reasons other than sign mismatch or overflow."},
			{10054, "The data value for one or more columns overflowed the type used by the provider."},
			{10060, " network-related or instance-specific error occurred while establishing a connection to SQL Server."},
			{20, "fatal errors"},
			{64, "The instance of SQL Server you attempted to connect to does not support encryption."}
		};

		public static IReadOnlyDictionary<int, string> Errors
		{
			get { return new ReadOnlyDictionary<int, string>(TransientErrors); }
		}
	}
}
