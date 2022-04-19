using System;

namespace Bau.Libraries.LibEncoder.Encoders
{
	/// <summary>
	///		Interface que deben cumplir las clases de codificación
	/// </summary>
	public interface IEncoder
	{
		/// <summary>
		///		Codifica una cadena 
		/// </summary>
		string Encode(string charSet, string source, bool subject);

		/// <summary>
		///		Codifica un array de bytes
		/// </summary>
		string Encode(string charSet, byte[] source, bool subject);

		/// <summary>
		///		Decodifica una cadena
		/// </summary>
		string Decode(string charSet, string source, bool subject);

		/// <summary>
		///		Decodifica un array de bytes en una cadena
		/// </summary>
		string Decode(string charSet, byte[] source, bool subject);

		/// <summary>
		///		Decodifica en un array de bytes una cadena
		/// </summary>
		byte[] DecodeToBytes(string charSet, string source, bool subject);

		/// <summary>
		///		Decodifica en un array de bytes otro array de bytes
		/// </summary>
		byte[] DecodeToBytes(string charSet, byte[] source, bool subject);
	}
}