using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnitTests
{
	public static class UnofficialTransientErrors
	{
		private static readonly List<KeyValuePair<int, string>> NonTransientErrors = new List<KeyValuePair<int, string>>
		{
			new KeyValuePair<int, string>(-2146232060, "A transport-level error has occurred when receiving results from the server. (provider: Session Provider, error: 19 - Physical connection is not usable)"),
			new KeyValuePair<int, string>(-2146232060, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.")
		};

		public static IReadOnlyCollection<KeyValuePair<int, string>> Errors
		{
			get { return new ReadOnlyCollection<KeyValuePair<int, string>>(NonTransientErrors); }
		}
	}
}
