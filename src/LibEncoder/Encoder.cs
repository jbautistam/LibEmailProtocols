using System;

namespace Bau.Libraries.LibEncoder
{
	/// <summary>
	///		Codificación
	/// </summary>
	public static class Encoder
	{
		/// <summary>
		///		Tipo de codificación / decodificación
		/// </summary>
		public enum EncoderType
		{
			Unknown,
			Base64,
			Bit7,
			Bit8,
			Dummy,
			QuotedPrintable
		}

		/// <summary>
		///		Obtiene el codificador
		/// </summary>
		public static Encoders.IEncoder GetEncoder(EncoderType encoder)
		{
			switch (encoder)
			{
				case EncoderType.Base64:
				return new Encoders.Base64Encoder();
				case EncoderType.Bit7:
				return new Encoders.Bit7Encoder();
				case EncoderType.Bit8:
				return new Encoders.Bit8Encoder();
				case EncoderType.Dummy:
				return new Encoders.DummyEncoder();
				case EncoderType.QuotedPrintable:
				return new Encoders.QuotedPrintableEncoder();
				default:
				throw new NotImplementedException();
			}
		}

		/// <summary>
		///		Obtiene el tipo de codificación
		/// </summary>
		public static EncoderType GetEncoderType(string transferEncoding)
		{
			if (string.IsNullOrEmpty(transferEncoding))
				return EncoderType.Dummy;
			else if (transferEncoding.Equals("8bit", StringComparison.CurrentCultureIgnoreCase))
				return EncoderType.Bit8;
			else if (transferEncoding.Equals("7bit", StringComparison.CurrentCultureIgnoreCase))
				return EncoderType.Bit7;
			else if (transferEncoding.Equals("QP", StringComparison.CurrentCultureIgnoreCase) ||
					 transferEncoding.Equals("quoted-printable", StringComparison.CurrentCultureIgnoreCase))
				return EncoderType.QuotedPrintable;
			else if (transferEncoding.Equals("Base64", StringComparison.CurrentCultureIgnoreCase))
				return EncoderType.Base64;
			else
				return EncoderType.Unknown;
		}

		/// <summary>
		///		Codifica una cadena
		/// </summary>
		public static string Encode(EncoderType encoder, string source)
		{
			return Encode(encoder, source, false);
		}

		/// <summary>
		///		Codifica una cadena
		/// </summary>
		public static string Encode(EncoderType encoder, string source, bool subject)
		{
			return GetEncoder(encoder).Encode(null, source, subject);
		}

		/// <summary>
		///		Decodifica una cadena
		/// </summary>
		public static string Decode(EncoderType encoder, string source, bool subject)
		{
			return Decode(encoder, null, source, subject);
		}

		/// <summary>
		///		Decodifica una cadena
		/// </summary>
		public static string Decode(EncoderType encoder, string charSet, string source, bool subject)
		{
			return GetEncoder(encoder).Decode(charSet, source, subject);
		}

		/// <summary>
		///		Decodifica una cadena sobre un archivo
		/// </summary>
		public static void DecodeToFile(EncoderType encoder, string source, string fileName)
		{
			DecodeToFile(encoder, null, source, fileName);
		}

		/// <summary>
		///		Decodifica una cadena sobre un archivo
		/// </summary>
		public static void DecodeToFile(EncoderType encoder, string charSet, string source, string fileName)
		{
			Encoders.Tools.FileHelper.Save(GetEncoder(encoder).DecodeToBytes(charSet, source, false), fileName);
		}

		/// <summary>
		///		Códifica un archivo (en base 64)
		/// </summary>
		public static string EncodeFileToBase64(string fileName)
		{
			string encoded = null;

				// Codifica el contenido del archivo
				using (System.IO.FileStream fnFile = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
																			  System.IO.FileAccess.Read))
				{
					byte[] buffer = new byte[fnFile.Length];

						// Lee el archivo
						fnFile.Read(buffer, 0, buffer.Length);
						// Obtiene la codificación del archivo
						encoded = Convert.ToBase64String(buffer, 0, buffer.Length);
						// Cierra el archivo
						fnFile.Close();
				}
				// Devuelve el contenido del archivo
				return encoded;
		}
	}
}