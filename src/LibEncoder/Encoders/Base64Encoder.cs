using System;
using System.Text;

namespace Bau.Libraries.LibEncoder.Encoders
{
	/// <summary>
	///		Codificador / decodificador en Base64
	/// </summary>
	public class Base64Encoder : IEncoder
	{
		/// <summary>
		///		Codifica una cadena (a base 64)
		/// </summary>
		public string Encode(string charSet, string source, bool subject)
		{
			return Encode(charSet, UTF8Encoding.UTF8.GetBytes(source), subject);
		}

		/// <summary>
		///		Codifica un array de bytes
		/// </summary>
		public string Encode(string charSet, byte[] source, bool subject)
		{
			return Convert.ToBase64String(source, Base64FormattingOptions.InsertLineBreaks);
		}

		/// <summary>
		///		Decodifica una cadena desde Base64 a otra cadena
		/// </summary>
		public string Decode(string charSet, string source, bool subject)
		{
			return Decode(charSet, DecodeToBytes(charSet, source, subject), subject);
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
			return Convert.FromBase64String(source);
		}

		/// <summary>
		///		Decodifica un array de bytes en un array de bytes
		/// </summary>
		public byte[] DecodeToBytes(string charSet, byte[] source, bool subject)
		{
			return DecodeToBytes(charSet, UTF8Encoding.UTF8.GetString(source), subject);
		}
	}
}