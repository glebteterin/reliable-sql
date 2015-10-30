using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Sql;

namespace UnitTests
{
	[TestFixture]
	public class SqlConnectionWrapperTests
	{
		private const string EmptyConnectionString = "";
		private const string FakeConnectionString = "Data Source=test\test;Initial Catalog=Test; Connect Timeout=1; Connection Timeout=1; Pooling=false; Persist Security Info=True;User ID=Test; password=test ";

		[Test]
		public void InvalidConnectionString()
		{
			var executor = InitExecutor(EmptyConnectionString);

			var testResult = executor.Execute(cnn =>
			{
				cnn.Open();
				return null;
			});

			Assert.That(testResult.ThrownException, Is.Not.Null);
			Assert.That(testResult.ThrownException, Is.TypeOf<AggregateException>());
			Assert.That(((AggregateException)testResult.ThrownException).InnerExceptions.Count, Is.EqualTo(10));
			Assert.That(testResult.AttemptCount, Is.EqualTo(10));
		}

		[Test]
		public void MockConnectionString()
		{
			var executor = InitExecutor(FakeConnectionString, TimeSpan.Zero, 1);

			var testResult = executor.Execute(cnn =>
			{
				cnn.Open();
				return null;
			});

			Assert.That(testResult.ThrownException, Is.Not.Null);
			Assert.That(testResult.ThrownException, Is.TypeOf<AggregateException>());
			Assert.That(((AggregateException)testResult.ThrownException).InnerExceptions.Count, Is.EqualTo(1));
			Assert.That(testResult.AttemptCount, Is.EqualTo(1));
		}

		private static TestExecutor InitExecutor(string connectionString, TimeSpan? delay = null, int? retries = null)
		{
			var connection = new SqlConnection(connectionString);

			var wrapper = default(SqlConnectionWrapper);

			if (delay.HasValue && retries.HasValue)
				wrapper = new SqlConnectionWrapper(connection, delay.Value, retries.Value);
			else
				wrapper = new SqlConnectionWrapper(connection);
				

			var executor = new TestExecutor(wrapper);

			return executor;
		}
	}
}
