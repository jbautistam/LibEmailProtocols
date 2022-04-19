using System;

using Bau.Libraries.LibEncoder;
using Bau.Libraries.LibMime.Components;
using Bau.Libraries.LibMime.Services.Helpers;

namespace Bau.Libraries.LibMime.Services
{
	/// <summary>
	///		Interpreta los datos de un mensaje de correo
	/// </summary>
	public class MimeMessageParser
	{
		/// <summary>
		///		Interpreta los datos de un mensaje
		/// </summary>
		public MimeMessage Parse(string message)
		{
			MimeMessage mail = new MimeMessage();
			ParserLines parser = new ParserLines(message);
			MimeHeadersParser headersParser = new MimeHeadersParser();
			Section section;

				// Interpreta las líneas recibidas
				headersParser.ReadHeaders(parser, mail);
				// Interpreta las secciones
				section = ParseSections(headersParser, mail.ContentType, parser);
				// Normaliza el correo
				NormalizeMail(mail, section);
				// Devuelve el mensaje recibido
				return mail;
		}

		/// <summary>
		///		Interpreta las secciones
		/// </summary>
		private Section ParseSections(MimeHeadersParser headersParser, ContentType contentType, ParserLines parser)
		{
			Section section = new Section();

				// Asigna los parámetros a la sección
				section.ContentType = contentType;
				// Interpreta la sección
				ParseSection(headersParser, section, parser);
				// Devuelve la sección interpretada
				return section;
		}

		/// <summary>
		///		Interpreta los datos de una sección
		/// </summary>
		private void ParseSection(MimeHeadersParser headersParser, Section section, ParserLines parser)
		{
			string lastLine = null;

				if (section.ContentType.IsMultipart)
					section.Sections.AddRange(ParseSectionsChild(headersParser, section.ContentType.Boundary, parser, ref lastLine));
				else if (section.ContentType.Type == ContentType.ContentTypeEnum.MultipartReport)
					section.Sections.AddRange(ParseSectionsMultipartReport(headersParser, section, parser, ref lastLine));
				else
					section.Content = ParseContentSection(parser, null, null, ref lastLine);
		}

		/// <summary>
		///		Interpreta las secciones con tipo de contenido "multipart/report"
		/// </summary>
		private SectionsCollection ParseSectionsMultipartReport(MimeHeadersParser headersParser, Section section, ParserLines parser,
																ref string lastLine)
		{
			return ParseSectionsChild(headersParser, section.ContentType.Boundary, parser, ref lastLine);
		}

		/// <summary>
		///		Interpreta las secciones hijo
		/// </summary>
		private SectionsCollection ParseSectionsChild(MimeHeadersParser headersParser, string boundaryParent, ParserLines parser,
													  ref string lastLine)
		{
			SectionsCollection sections = new SectionsCollection();
			string line;

				// Lee la primera línea (o recupera la anterior
				if (!string.IsNullOrEmpty(lastLine))
					line = lastLine;
				else
					line = parser.ReadLine();
				// Quita las líneas anteriores al boundary si es necesario
				if (!string.IsNullOrEmpty(boundaryParent) && !IsStartBoundary(line, boundaryParent))
				{ 
					// Lee la siguiente línea
					while (!parser.IsEof && !IsStartBoundary(line, boundaryParent))
						line = parser.ReadLine();
				}
				// Si es la línea de boundary
				if (IsStartBoundary(line, boundaryParent))
				{
					bool end = false;

						// Recorre las secciones hija
						while (!parser.IsEof && !end)
							if (!string.IsNullOrEmpty(lastLine) && IsEndBoundary(lastLine, boundaryParent))
								end = true;
							else
							{
								Section section = ParseSection(headersParser, parser, boundaryParent, ref lastLine);

									// Añade la sección a la colección
									sections.Add(section);
									// Comprueba la última línea leída
									if (IsEndBoundary(lastLine, boundaryParent))
									{ 
										// Vacía la última línea al ser un final de sección para pasar a la siguiente lectura
									  // sin una cadena de lectura atrasada (al fin y al cabo, la hemos tratado en el
									  // IsEndBoundary
										if (!parser.IsEof)
										{
											lastLine = parser.ReadLine();
											if (lastLine == "")
												lastLine = parser.ReadLine();
										}
										else
											lastLine = "";
										// Indica que es el final de sección
										end = true;
									}
									else if (section.ContentType.IsMultipart)
										section.Sections.AddRange(ParseSectionsChild(headersParser, section.ContentType.Boundary,
																					 parser, ref lastLine));
							}
				}
				else
					throw new Exception("Boundary mal formado");
				// Devuelve las secciones
				return sections;
		}

		/// <summary>
		///		Interpreta la sección
		/// </summary>
		private Section ParseSection(MimeHeadersParser headersParser, ParserLines parser, string boundaryParent, ref string lastLine)
		{
			Section section = new Section();

				// Lee las cabeceras
				section.Headers = headersParser.ReadHeaders(parser, section);
				// Obtiene el contenido de la sección
				section.Content = ParseContentSection(parser, boundaryParent, section.ContentType.Boundary, ref lastLine);
				// Devuelve la sección
				return section;
		}

		/// <summary>
		///		Interpreta el contenido de una sección
		/// </summary>
		private string ParseContentSection(ParserLines parser, string boundaryParent, string boundary, ref string lastLine)
		{
			bool end = false;
			string line;
			System.Text.StringBuilder sbContent = new System.Text.StringBuilder();

				// Lee el contenido
				while (!end && !parser.IsEof)
				{ 
					// Lee la línea
					line = parser.ReadLine();
					// Dependiendo de si estamos en una línea de boundary o en una línea de contenido
					if (IsStartBoundary(line, boundary) || IsStartBoundary(line, boundaryParent))
					{ 
						// Guarda la última línea
						lastLine = line;
						// Indica que ha terminado
						end = true;
					}
					else
					{ // Añade el salto de línea si es necesario
						if (sbContent.Length > 0)
							sbContent.Append("\r\n");
						// Añade la línea al contenido
						sbContent.Append(line);
					}
				}
				// Devuelve el contenido de la sección
				return sbContent.ToString();
		}

		/// <summary>
		///		Comprueba si la línea es un limitador de inicio
		/// </summary>
		private bool IsStartBoundary(string line, string boundary)
		{
			if (string.IsNullOrEmpty(boundary))
				return false;
			else if (string.IsNullOrEmpty(line))
				return false;
			else
				return line.StartsWith("--") && line.IndexOf(boundary) >= 0;
		}

		/// <summary>
		///		Comprueba si la línea es un limitador de fin
		/// </summary>
		private bool IsEndBoundary(string line, string boundary)
		{
			return IsStartBoundary(line, boundary) && line.EndsWith("--");
		}

		/// <summary>
		///		Normaliza el correo
		/// </summary>
		private void NormalizeMail(MimeMessage mail, Section section)
		{   
			// Normaliza la sección
			if (section.ContentType.Type == ContentType.ContentTypeEnum.Text)
			{
				mail.Body = section;
				mail.Body.Content = Encoder.Decode(Encoder.GetEncoderType(section.TransferEncoding.TransferEncodingDefinition),
																		  section.ContentType.CharSet,
																		  section.Content, false);
			}
			else if (section.ContentType.Type == ContentType.ContentTypeEnum.HTML)
			{
				mail.BodyHTML = section;
				mail.BodyHTML.Content = Encoder.Decode(Encoder.GetEncoderType(section.TransferEncoding.TransferEncodingDefinition),
																			  section.ContentType.CharSet,
																			  section.Content, false);
			}
			else if (section.ContentDisposition.IsAttachment)
				mail.Attachments.Add(section);
			// Normaliza las secciones hija
			foreach (Section sectionChild in section.Sections)
				NormalizeMail(mail, sectionChild);
		}
	}
}