namespace GlebTeterin.ReliableSql
{
	internal class Constants
	{
		public const string TraceSourceName = "GlebTeterin.ReliableSql";

		public const string ExceptionAttempts = "GlebTeterin.ReliableSql.Attempts";
		public const string ExceptionFirstOccurrence = "GlebTeterin.ReliableSql.FirstOccurrence";

		public const int DefaultMaxRetries = 10;
		public const int DefaultDelayMs = 100;
	}
}
