using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class SqlDatabaseTransientErrorDetectionStrategyTests
	{
		private readonly SqlDatabaseTransientErrorDetectionStrategy _strategy
			= new SqlDatabaseTransientErrorDetectionStrategy();

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
		public void UnofficialTransientErrors_ShouldNotBeHandled()
		{
			var transientErrors = Helper.GenerateExceptions(UnofficialTransientErrors.Errors);

			foreach (var transientError in transientErrors)
			{
				Assert.That(_strategy.IsTransient(transientError),
					Is.False,
					"SqlException with Message '{0}' are not handled by stock SqlDatabaseTransientErrorDetectionStrategy. "
					+ "Check version of TransientFaultHandling library.",
					transientError.Message);
			}
		}
	}
}
