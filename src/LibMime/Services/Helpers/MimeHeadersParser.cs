using System;

using Bau.Libraries.LibMime.Components;

namespace Bau.Libraries.LibMime.Services.Helpers
{
	/// <summary>
	///		Intérprete de las cabeceras de un mensaje Mime
	/// </summary>
	internal class MimeHeadersParser
	{
		/// <summary>
		///		Lee las cabeceras del mensaje
		/// </summary>
		internal void ReadHeaders(ParserLines parser, MimeMessage message)
		{
			// Lee las cabeceras
			message.Headers.AddRange(ReadHeaders(parser));
			// Normaliza las cabeceras del mensaje
			NormalizeHeaders(message);
		}

		/// <summary>
		///		Lee las cabeceras de una sección
		/// </summary>
		internal HeadersCollection ReadHeaders(ParserLines parser, Section section)
		{
			HeadersCollection headers = ReadHeaders(parser);

				// Normaliza las cabeceras
				NormalizeHeaders(section, headers);
				// Devuelve la colección de cabeceras
				return headers;
		}

		/// <summary>
		///		Lee las cabeceras
		/// </summary>
		private HeadersCollection ReadHeaders(ParserLines parser)
		{
			HeadersCollection headers = new HeadersCollection();
			string line = "----";

				// Interpreta las líneas de cabecera
				while (!parser.IsEof && !string.IsNullOrEmpty(line))
				{
					// Obtiene la línea
					line = parser.ReadLineContinuous();
					// Interpreta la cabecera (si la hay)
					if (!string.IsNullOrEmpty(line))
						headers.Add(ParseHeader(line));
				}
				// Devuelve la colección de cabeceras
				return headers;
		}

		/// <summary>
		///		Interpreta una cabecera a partir de la línea
		/// </summary>
		private Header ParseHeader(string line)
		{
			Header header = new Header();
			int index = line.IndexOf(":");

				// Separa la clave del nombre en la cabecera
				if (index >= 0)
				{ 
					// Asigna los datos de la cabecera
					header.Name = Trim(line.Substring(0, index));
					header.Value = Trim(line.Substring(index + 1));
					// Interpreta el valor de la cabecera
					if (!string.IsNullOrEmpty(header.Value) &&
							!header.Name.Equals("Subject", StringComparison.CurrentCultureIgnoreCase) &&
							header.Value.IndexOf(';') >= 0)
					{
						string[] parameters = header.Value.Split(';');

							// Asigna el nuevo valor
							if (parameters.Length > 0)
							{ 
								// Asigna el nuevo valor a la cabecera
								header.Value = parameters[0];
								// Asigna las subcabeceras
								for (int subHeader = 1; subHeader < parameters.Length; subHeader++)
								{
									int position = parameters[subHeader].IndexOf('=');

										// Añade la subcabecera
										if (position <= 0)
											header.SubHeaders.Add(Header.Undefined, parameters[subHeader]);
										else
											header.SubHeaders.Add(parameters[subHeader].Substring(0, position),
																  parameters[subHeader].Substring(position + 1));
								}
							}
					}
				}
				// Quita las comillas de las subcabeceras
				foreach (Header subHeader in header.SubHeaders)
				{
					if (subHeader.Value.StartsWith("\""))
						subHeader.Value = subHeader.Value.Substring(1);
					if (subHeader.Value.EndsWith("\""))
						subHeader.Value = subHeader.Value.Substring(0, subHeader.Value.Length - 1);
				}
				// Imprime la cabecera
				//System.Diagnostics.Debug.WriteLine("------------------------------------------------");						
				//System.Diagnostics.Debug.WriteLine("   " + header.Name + ": " + header.Value);
				//foreach (Header objSubHeader in header.SubHeaders)	
				//  System.Diagnostics.Debug.WriteLine("                --> " + objSubHeader.Name + ": " + objSubHeader.Value);
				// Devuelve la cabecera
				return header;
		}

		/// <summary>
		///		Normaliza las cabeceras de un correo
		/// </summary>
		private void NormalizeHeaders(MimeMessage mail)
		{
			foreach (Header header in mail.Headers)
				if (header.Name.Equals(Header.From, StringComparison.CurrentCultureIgnoreCase))
					mail.From = new Address(DecodeQP(header.Value, true));
				else if (header.Name.Equals(Header.To, StringComparison.CurrentCultureIgnoreCase))
					mail.To = new AddressesCollection(DecodeQP(header.Value, true));
				else if (header.Name.Equals(Header.CC, StringComparison.CurrentCultureIgnoreCase))
					mail.CC = new AddressesCollection(DecodeQP(header.Value, true));
				else if (header.Name.Equals(Header.Subject, StringComparison.CurrentCultureIgnoreCase))
					mail.Subject = DecodeQP(header.Value, true);
				else if (header.Name.Equals(Header.Date, StringComparison.CurrentCultureIgnoreCase))
					mail.Date = GetDate(header.Value);
				else if (header.Name.Equals(Header.MimeVersion, StringComparison.CurrentCultureIgnoreCase))
					mail.MimeVersion = header.Value;
				else if (header.Name.Equals(Header.MessageID, StringComparison.CurrentCultureIgnoreCase))
					mail.ID = header.Value;
				else if (header.Name.Equals(Header.ContentType, StringComparison.CurrentCultureIgnoreCase))
					mail.ContentType = ParseContentType(header);
				else if (header.Name.Equals(Header.ContentTransferEncoding, StringComparison.CurrentCultureIgnoreCase))
					mail.TransferEncoding = ParseTransferEncoding(header);
		}

		/// <summary>
		///		Decodifica una cadena en QuotedPrintable
		/// </summary>
		private string DecodeQP(string source, bool isSubject)
		{
			return LibEncoder.Encoder.Decode(LibEncoder.Encoder.EncoderType.QuotedPrintable, source, isSubject);
		}

		/// <summary>
		///		Normaliza una serie de cabeceras estándar para el correo y las secciones
		/// </summary>
		internal void NormalizeHeaders(Section section, HeadersCollection headers)
		{
			foreach (Header header in headers)
				if (header.Name.Equals(Header.ContentType, StringComparison.CurrentCultureIgnoreCase))
					section.ContentType = ParseContentType(header);
				else if (header.Name.Equals(Header.ContentTransferEncoding, StringComparison.CurrentCultureIgnoreCase))
					section.TransferEncoding = ParseTransferEncoding(header);
				else if (header.Name.Equals(Header.ContentDisposition, StringComparison.CurrentCultureIgnoreCase))
					section.ContentDisposition = ParseContentDisposition(header);
		}

		/// <summary>
		///		Interpreta el ContentType de un elemento
		/// </summary>
		private ContentType ParseContentType(Header header)
		{
			ContentType contentType = new ContentType();

				// Asigna la definición
				contentType.ContentTypeDefinition = header.SearchValue(Header.ContentType);
				contentType.Boundary = header.SearchValue(Header.ContentTypeBoundary);
				contentType.CharSet = header.SearchValue(Header.CharSet);
				// Devuelve el tipo de contenido
				return contentType;
		}

		/// <summary>
		///		Interpreta la forma de transferencia
		/// </summary>
		private ContentTransfer ParseTransferEncoding(Header header)
		{
			ContentTransfer transfer = new ContentTransfer();

				// Asigna los datos
				transfer.TransferEncodingDefinition = header.Value;
				// Devuelve el modo de transferencia
				return transfer;
		}

		/// <summary>
		///		Interpreta la disposición de la sección
		/// </summary>
		/// <example>
		///		Content-Disposition: attachment; filename="N35MQ55Z.pdf"
		/// </example>
		private ContentDisposition ParseContentDisposition(Header header)
		{
			ContentDisposition disposition = new ContentDisposition();

				// Si tenemos una disposición en el valor ...
				disposition.ContentDispositionDefinition = header.SearchValue(Header.ContentDisposition);
				disposition.FileName = header.SearchValue(Header.FileName);
				// Devuelve la disposición de la sección
				return disposition;
		}

		/// <summary>
		///		Trim de una cadena
		/// </summary>
		private string Trim(string value)
		{
			if (!string.IsNullOrEmpty(value))
				return value.Trim();
			else
				return value;
		}

		/// <summary>
		///		Obtiene la fecha a partir de una cadena
		/// </summary>
		private DateTime GetDate(string date)
		{
			if (!DateTime.TryParse(date, out DateTime dtmValue))
				return DateTime.MinValue;
			else
				return dtmValue;
		}
	}
}
