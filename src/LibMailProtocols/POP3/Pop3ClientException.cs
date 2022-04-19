using System;

namespace Bau.Libraries.LibMailProtocols.POP3
{
	/// <summary>
	///		Excepción de comunicación con un servidor POP3
	/// </summary>
	public class Pop3ClientException : Exception
	{
		public Pop3ClientException() : this(null, null) { }

		public Pop3ClientException(string message) : this(message, null) { }

		public Pop3ClientException(string message, Exception innerException) : base(message, innerException) { }

		protected Pop3ClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
