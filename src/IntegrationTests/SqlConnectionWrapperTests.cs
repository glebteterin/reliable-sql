using System;
using System.Data.SqlClient;
using NUnit.Framework;

namespace IntegrationTests
{
	[TestFixture]
	public class SqlConnectionWrapperTests
	{
		private const string EmptyConnectionString = "";
		private const string FakeConnectionString = "Data Source=test\test;Initial Catalog=Test; Connect Timeout=1; Connection Timeout=1; Pooling=false; Persist Security Info=True;User ID=Test; password=test ";

		[Test]
		public void InvalidConnectionString()
		{
			var executor = InitExecutor(EmptyConnectionString, TimeSpan.Zero, 2);

			var testResult = executor.Execute(Guid.NewGuid(), 2, "NOTUSABLE");

			Assert.That(testResult.ThrownException, Is.Not.Null);
			Assert.That(testResult.ThrownException, Is.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void MockConnectionString()
		{
			var executor = InitExecutor(FakeConnectionString, TimeSpan.Zero, 2);

			var testResult = executor.Execute(Guid.NewGuid(), 2, "NOTUSABLE");

			Assert.That(testResult.ThrownException, Is.Not.Null);
			Assert.That(testResult.ThrownException, Is.TypeOf<SqlException>());
		}

		[Test]
		public void SqlExcption_NotUsable()
		{
			var executor = InitExecutor(Config.ConnectionString, TimeSpan.Zero, 1);

			var testResult = executor.Execute(Guid.NewGuid(), 2, "NOTUSABLE");

			Assert.That(testResult.ThrownException, Is.Not.Null);
			Assert.That(testResult.ThrownException, Is.TypeOf<AggregateException>());
			Assert.That(((AggregateException)testResult.ThrownException).InnerExceptions.Count, Is.EqualTo(1));
			Assert.That(((AggregateException)testResult.ThrownException).InnerExceptions[0].Message.Contains("not usable"));
		}

		private static TestExecutor InitExecutor(string connectionString, TimeSpan delay, int retries)
		{
			var executor = new TestExecutor(connectionString, delay, retries);

			return executor;
		}
	}
}
