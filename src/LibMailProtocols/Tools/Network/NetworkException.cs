using System;

namespace Bau.Libraries.LibMailProtocols.Tools.Network
{
	/// <summary>
	///		Excepciones de red
	/// </summary>
	public class NetworkException : Exception
	{
		public NetworkException(string message) : this(message, null) { }

		public NetworkException(string message, Exception innerException) : base(message, innerException) { }

		public NetworkException() : base()
		{
		}

		protected NetworkException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
