using System;
using System.Security.Cryptography;
using System.Text;

namespace Bau.Libraries.LibMailProtocols.Tools.Cryptography
{
	/// <summary>
	///		Clase para codificación MD5
	/// </summary>
	internal static class MD5Helper
	{
		/// <summary>
		///		Calcula una cadena MD5 a partir de una cadena de entrada
		/// </summary>
		internal static string Compute(string source)
		{
			byte [] target;
			MD5 provider = new MD5CryptoServiceProvider();

				// Codifica la cadena
				target = provider.ComputeHash(ASCIIEncoding.Default.GetBytes(source));
				// Convierte los bytes codificados en una cadena legible
				return BitConverter.ToString(target).Replace("-", "");
		}
	}
}
