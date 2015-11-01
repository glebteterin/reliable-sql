using System.Configuration;

namespace IntegrationTests
{
	public class Config
	{
		public static string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["ConnectionString"]; }
		}
	}
}
