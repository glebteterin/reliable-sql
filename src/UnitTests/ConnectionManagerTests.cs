using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using GlebTeterin.ReliableSql;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class ConnectionManagerTests
	{
		private const string EmptyConnectionString = "";

		[Test]
		public void ShouldProvideRetryDetailsInExceptionData()
		{
			const int expectedAttempts = 2;

			var cm = new ConnectionManager(
				EmptyConnectionString,
				new TransientErrorCatchAllStrategy(),
				expectedAttempts,
				TimeSpan.FromMilliseconds(1)
				);

			var actualAttempts = 0;
			var actualAttemptsFromException = 0;
			var actualFirstOccurrence = DateTime.MinValue;

			cm.Retrying += (sender, args) => ++actualAttempts;

			try
			{
				cm.Execute(cnn => cnn.Open());
			}
			catch (Exception ex)
			{
				actualAttemptsFromException = (int) ex.Data["GlebTeterin.ReliableSql.Attempts"];
				actualFirstOccurrence = (DateTime) ex.Data["GlebTeterin.ReliableSql.FirstOccurrence"];
			}

			Assert.That(actualAttempts, Is.EqualTo(expectedAttempts));
			Assert.That(actualAttemptsFromException, Is.EqualTo(expectedAttempts));
			Assert.That(actualFirstOccurrence, Is.GreaterThan(DateTime.MinValue));
		}
	}
}
