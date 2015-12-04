using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace UnitTests
{
	public class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			return true;
		}
	}
}
