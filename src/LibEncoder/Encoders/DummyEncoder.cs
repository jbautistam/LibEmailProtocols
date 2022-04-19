using System;
using System.Text;

namespace Bau.Libraries.LibEncoder.Encoders
{
	/// <summary>
	///		Codificador / decodificador en 8Bit
	/// </summary>
	public class DummyEncoder : IEncoder
	{
		/// <summary>
		///		Codifica una cadena (a base 64)
		/// </summary>
		public string Encode(string charSet, string source, bool subject)
		{
			return source;
		}

		/// <summary>
		///		Codifica un array de bytes
		/// </summary>
		public string Encode(string charSet, byte[] source, bool subject)
		{
			return Convert.ToString(source);
		}

		/// <summary>
		///		Decodifica una cadena desde Base64 a otra cadena
		/// </summary>
		public string Decode(string charSet, string source, bool subject)
		{
			return source;
		}

		/// <summary>
		///		Decodificar una cadena desde un array de bytes
		/// </summary>
		public string Decode(string charSet, byte[] source, bool subject)
		{
			return UTF8Encoding.UTF8.GetString(source);
		}

		/// <summary>
		///		Decodifica una cadena en base 64 a un array de bytes
		/// </summary>
		public byte[] DecodeToBytes(string charSet, string source, bool subject)
		{
			return ASCIIEncoding.ASCII.GetBytes(source);
		}

		/// <summary>
		///		Decodifica un array de bytes en un array de bytes
		/// </summary>
		public byte[] DecodeToBytes(string charSet, byte[] source, bool subject)
		{
			return source;
		}
	}
}