using NUnit.Framework;
using GlebTeterin.ReliableSql;

namespace UnitTests
{
	[TestFixture]
	public class ExtendedStrategyTests
	{
		private readonly AzureSqlStrategy _strategy = new AzureSqlStrategy();

		[Test]
		public void OfficialTransientErrors_ShouldBeHandled()
		{
			var transientErrors = Helper.GenerateExceptions(OfficialTransientErrors.Errors);

			foreach (var transientError in transientErrors)
			{
				Assert.That(_strategy.IsTransient(transientError),
					"SqlException with ErrorNumber {0} should be handled as transient.",
					transientError.ErrorCode);
			}
		}

		[Test]
		public void UnofficialTransientErrors_ShouldBeHandled()
		{
			var transientErrors = Helper.GenerateExceptions(UnofficialTransientErrors.Errors);

			foreach (var transientError in transientErrors)
			{
				Assert.That(_strategy.IsTransient(transientError),
					"SqlException with ErrorNumber {0} should be handled as transient.",
					transientError.Message);
			}
		}
	}
}
