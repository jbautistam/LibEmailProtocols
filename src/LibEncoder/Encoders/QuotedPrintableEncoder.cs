using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibEncoder.Encoders
{
	/// <summary>
	///		Implementación del codificador en formato QuotedPrintable
	/// </summary>
	public class QuotedPrintableEncoder : IEncoder
	{  
		// Constantes privadas
		private const int CharsPerLine = 75; // Número máximo de caracteres por línea sin incluir los saltos de línea
		private const string EndOfLine = "\r\n";
		private const string EqualSign = "=";
		private const string HexPattern = "(\\=([0-9A-F][0-9A-F]))";

		/// <summary>
		///		Codifica una cadena con un charset determinado
		/// </summary>
		public string Encode(string charSet, string source, bool subject)
		{
			if (IsNonAscii(source))
				return EncodeQP(source, charSet, false, subject);
			else
				return source;
		}

		/// <summary>
		///		Codifica un array de bytes
		/// </summary>
		public string Encode(string charSet, byte[] source, bool subject)
		{
			return Encode(charSet, UTF8Encoding.UTF8.GetString(source, 0, source.Length), subject);
		}

		/// <summary>
		///		Decodifica una cadena
		/// </summary>
		public string Decode(string charSet, string source, bool subject)
		{
			return DecodeQP(charSet, source, subject);
		}

		/// <summary>
		///		Decodifica una cadena desde un array de bytes
		/// </summary>
		public string Decode(string charSet, byte[] source, bool subject)
		{
			return Decode(charSet, UTF8Encoding.UTF8.GetString(source, 0, source.Length), subject);
		}

		/// <summary>
		///		Decodifica una cadena en un array de bytes
		/// </summary>
		public byte[] DecodeToBytes(string charSet, string source, bool subject)
		{
			return ASCIIEncoding.ASCII.GetBytes(Decode(charSet, source, subject));
		}

		/// <summary>
		///		Decodifica un array de bytes en un array de bytes
		/// </summary>
		public byte[] DecodeToBytes(string charSet, byte[] source, bool subject)
		{
			return DecodeToBytes(charSet, ASCIIEncoding.ASCII.GetString(source), subject);
		}

		/// <summary>
		///		Codifica una cadena en el formato QuotedPrintable
		/// </summary>
		private string EncodeQP(string source, string charSet, bool forceRFC2047, bool subject)
		{
			StringBuilder target = new StringBuilder();
			StringReader reader = new StringReader(source);
			string line = null;
			bool forceEncoding = false;
			int columnPosition;

				// Inicializa la columna
				columnPosition = target.Length;
				// Codifica la cadena
				while ((line = reader.ReadLine()) != null)
				{
					Encoding encoding = GetEncoderText(charSet);
					byte[] lineEncoded = encoding.GetBytes(line);
					StringBuilder blankendChars = new StringBuilder();
					int endPos = lineEncoded.Length - 1;

						// Codifica los caracteres en blanco
						while (endPos >= 0 && (lineEncoded[endPos] == 0x20 || lineEncoded[endPos] == 0x09))
						{
							blankendChars.Insert(blankendChars.Length, EncodeByte(lineEncoded[endPos]));
							endPos--;
						}
						// Añade los caracteres codificados
						for (int index = 0; index <= endPos; index++)
						{
							string toWrite = "";

								// Añade el carácter (codificado o no)
								if (forceEncoding || NeedsEncoding(lineEncoded[index], forceRFC2047))
								{
									columnPosition += 3;
									toWrite = EncodeByte(lineEncoded[index]);
								} 
								else
								{
									columnPosition++;
									toWrite = encoding.GetString(new byte[] { lineEncoded[index] });
								}
								// Añade el salto de línea
								if (columnPosition > QuotedPrintableEncoder.CharsPerLine)
								{
									target.Append("=" + QuotedPrintableEncoder.EndOfLine);
									columnPosition = toWrite.Length;
								}
								// Añade la cadena al buffer total
								target.Append(toWrite);
						}
						// Añade los caracteres finales
						if (blankendChars.Length > 0)
						{
							if (columnPosition + blankendChars.Length > QuotedPrintableEncoder.CharsPerLine)
								target.Append("=" + QuotedPrintableEncoder.EndOfLine);
							target.Append(blankendChars);
						}
						// Añade un salto de línea
						if (reader.Peek() >= 0)
							target.Append(EndOfLine);
						// Inicializa la columna
						columnPosition = 0;
				}
				// Devuelve la cadena codificada
				return target.ToString();
		}

		/// <summary>
		///		Obtiene el codificador de texto
		/// </summary>
		private Encoding GetEncoderText(string charSet)
		{
			if (string.IsNullOrEmpty(charSet))
				return Encoding.GetEncoding("utf-8");
			else
				return Encoding.GetEncoding(charSet);
		}

		/// <summary>
		///		Codifica una cadena como asunto de mensaje
		/// </summary>
		private string EncodeSubject(string subject, string charSet)
		{
			if (IsNonAscii(subject))
			{
				string content = "";

					// Reemplaza los saltos de línea
					subject = subject.Replace("\r\n", null);
					subject = subject.Replace("\n", null);
					// Codifica el texto
					subject = Encode(subject, charSet, false);
					// Lee por líneas y le añade los datos
					using (StringReader reader = new StringReader(subject))
					{
						bool first = true;
						string line;

							// Añade la cabecera
							content = "=?" + charSet + "?Q?";
							// Lee las líneas
							while ((line = reader.ReadLine()) != null)
							{ 
								// Añade el salto de línea si es necesario
								if (!first)
									content += "\r\n =";
								// Añade la línea
								content += line;
								// Indica que ya no es la primera línea
								first = false;
							}
					}
				// Añade el fin de código y lo devuelve
				return content + "?=";
			} 
			else
				return subject;
		}

		/// <summary>
		///		Codifica un carácter
		/// </summary>
		private string EncodeChar(char chr)
		{
			int intChar = (int) chr;

				// Devuelve el carácter codificado
				if (intChar > 255)
					return string.Format("={0:X2}={1:X2}", intChar >> 8, intChar & 0xFF);
				else
					return string.Format("={0:X2}", chr);
		}

		/// <summary>
		///		Codifica un byte
		/// </summary>
		private string EncodeByte(byte chr)
		{
			return string.Format("={0:X2}", (int) chr);
		}

		/// <summary>
		///		Comprueba si se debe codificar un carácter
		/// </summary>
		internal bool NeedsEncoding(char chr, bool forceRFC2047)
		{
			return (IsNonAscii(chr) || (forceRFC2047 && (chr == 0x20 || chr == 0x09 || chr == 0x3f)));
		}

		/// <summary>
		///		Comprueba si se debe codificar un carácter
		/// </summary>
		internal bool NeedsEncoding(byte chr, bool forceRFC2047)
		{
			return NeedsEncoding((char) chr, forceRFC2047);
		}

		/// <summary>
		///		Comprueba si en una cadena hay algún carácter no ASCII
		/// </summary>
		private bool IsNonAscii(string source)
		{ 
			// Comprueba los caracteres de la cadena
			foreach (char chr in source)
				if (IsNonAscii(chr))
					return true;
			// Si ha llegado hasta aquí es porque todos los caracteres son ASCII
			return false;
		}

		/// <summary>
		///		Se considera que un carácter no es ASCII si su código está por debajo del del espacio
		///	(excepto el tabulador) o es un signo igual o el código es mayor o igual a 0x7F
		/// </summary>
		private bool IsNonAscii(char chr)
		{
			return (chr <= 0x1F && chr != 0x09) || chr == 0x3D || chr >= 0x7F;
		}

		/// <summary>
		///		Decodifica una cadena
		/// </summary>
		private string DecodeQP(string charSet, string text, bool subject)
		{
			StringBuilder writer = new StringBuilder();
			string target;

				// Decodifica el mensaje
				using (StringReader reader = new StringReader(SplitQuotedPrintable(text, subject)))
				{
					string line;

						while ((line = reader.ReadLine()) != null)
						{ 
							// Elimina los blancos finales
							line = line.TrimEnd();
							// Elimina el salto de línea final
							if (line.EndsWith("="))
							{ 
								// Quita el fin de línea
								if (line.Length > 1)
									line = line.Substring(0, line.Length - 1);
								else
									line = "";
								// Añade la línea sin salto de línea
								writer.Append(DecodeLine(charSet, line, subject));
							} 
							else
								writer.AppendLine(DecodeLine(charSet, line, subject));
						}
				}
				// Si es el asunto de un mensaje quita los saltos de línea y sustituye los subrayados
				target = writer.ToString();
				if (!string.IsNullOrEmpty(target) && subject)
				{
					target = target.Replace("\r\n", " ");
					target = target.Replace("_", " ");
				}
				// Devuelve la cadena codificada
				return target;
		}

		/// <summary>
		///		Decodifica una línea
		/// </summary>
		private string DecodeLine(string charSet, string line, bool subject)
		{
			Regex regex = new Regex(HexPattern, RegexOptions.IgnoreCase);

				// Obtiene el charset del cabecera
				if (subject && string.IsNullOrEmpty(charSet))
					charSet = GetCharSet(ref line);
				// Decodifica la línea
				if (!string.IsNullOrEmpty(charSet) && charSet.Equals("utf-8", StringComparison.CurrentCultureIgnoreCase))
					return DecodeQuotedPrintableUTF8(line);
				else
					return regex.Replace(line, new MatchEvaluator(HexMatchEvaluator));
		}

		/// <summary>
		///		Decodifica un carácter
		/// </summary>
		private string HexMatchEvaluator(Match match)
		{
			return Convert.ToChar(Convert.ToInt32(match.Groups[2].Value, 16)).ToString();
		}

		/// <summary>
		///		Obtiene el charSet de una línea y la elimina
		/// </summary>
		private string GetCharSet(ref string line)
		{
			string charSet = "";

				// Quita la cabecera del charSet
				if (line.StartsWith("=?"))
				{
					int index;

						// Quita el inicio
						line = line.Substring(2);
						// Quita el charSet
						index = line.IndexOf("?");
						if (index >= 0)
						{ 
							// Obtiene la codificación
							charSet = line.Substring(0, index);
							// Quita el charSet
							line = line.Substring(index);
							// Quita los caracteres ?Q? que identifican la cabecera
							line = line.Substring(3);
						}
				}
				// Devuelve el charSet
				return charSet;
		}

		/// <summary>
		///		Decodifica QuotedPrintable con UTF-8
		/// </summary>
		private string DecodeQuotedPrintableUTF8(string source)
		{
			string output = "";

				// Decodifica los caracteres
				while (!string.IsNullOrEmpty(source))
				{
					int index = source.IndexOf("=");

						if (index < 0)
						{
							output += source;
							source = "";
						} 
						else
						{
							output += source.Substring(0, index);
							source = source.Substring(index);
							if (source.StartsWith("=?="))
							{
								output += " ";
								source = source.Substring(3);
							} 
							else if (source.StartsWith("=") && source.Length > 3 && source.Substring(3, 1) == "=")
							{
								UTF8Encoding encoding = new UTF8Encoding();
								string[] arrStrHex = source.Substring(0, 6).Split('=');
								string strHex2 = source.Substring(4, 2);
								byte[] buffer = new byte[2];

								buffer[0] = Convert.ToByte(int.Parse(arrStrHex[1], System.Globalization.NumberStyles.HexNumber));
								buffer[1] = Convert.ToByte(int.Parse(arrStrHex[2], System.Globalization.NumberStyles.HexNumber));
								output += encoding.GetString(buffer);
								// Quita los cinco primeros caracteres
								source = source.Substring(6);
							} 
							else if (source.Length >= 3)
							{
								UTF8Encoding encoding = new UTF8Encoding();
								string hexa = source.Substring(1, 2);

									if (hexa != "\r\n")
									{
										byte[] buffer = new byte[1];

											buffer[0] = Convert.ToByte(int.Parse(hexa, System.Globalization.NumberStyles.HexNumber));
											output += encoding.GetString(buffer);
									} 
									else
										output += "\r\n";
									// Quita los tres primeros caracteres
									source = source.Substring(3);
							} 
							else
							{
								output += source;
								source = "";
							}
						}
				}
				// Devuelve el resultado
				return output;
		}

		/// <summary>
		///		Añade saltos de línea reemplazando los ?= que indican el final de una línea codificada como QuotedPrintable
		/// </summary>
		private string SplitQuotedPrintable(string message, bool subject)
		{ 
			// En los asuntos, reemplaza los caracteres ?=
			if (subject && !string.IsNullOrEmpty(message))
			{ 
				// Sustituye los ?= con un espacio final por = + salto de línea para que al interpretar el asunto
				// se ponga en la misma línea
				message = message.Replace("?= ", "=\r\n");
				// Reemplaza el ?= final
				if (message.EndsWith("?="))
					message = message.Substring(0, message.Length - 2);
			}
			// Devuelve el mensaje
			return message;
		}

		/// <summary>
		///		Trim de una línea
		/// </summary>
		private string Trim(string line)
		{
			if (string.IsNullOrEmpty(line))
				return "";
			else
				return line.Trim();
		}
	}
}