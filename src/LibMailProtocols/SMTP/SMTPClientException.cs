using System;

namespace Bau.Libraries.LibMailProtocols.SMTP
{
	/// <summary>
	///		Excepción de comunicación con un servidor SMTP
	/// </summary>
	public class SMTPClientException : Exception
	{
		public SMTPClientException() : this(null, null) { }

		public SMTPClientException(string message) : this(message, null) { }

		public SMTPClientException(string message, Exception innerException) : base(message, innerException) { }

		protected SMTPClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
