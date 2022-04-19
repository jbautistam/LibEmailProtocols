using System;
using System.Text;

using BauEncoder = Bau.Libraries.LibEncoder;
using Bau.Libraries.LibMime.Components;

namespace Bau.Libraries.LibMime.Services
{
	/// <summary>
	///		Creador de mensajes
	/// </summary>
	public class MimeMessageCreator
	{ 
		// Constantes privadas
		private const int MaxCharsPerLine = 76;
		private const string EndLine = "\r\n";

		/// <summary>
		///		Crea el mensaje a enviar
		/// </summary>
		public string CreateMessage(MimeMessage message)
		{
			return CreateMessage(message, "iso-8859-15");
		}

		/// <summary>
		///		Crea la cabecera del mensaje
		/// </summary>
		public string CreateMessage(MimeMessage message, string charSet)
		{
			StringBuilder writer = new StringBuilder();

				// Responder a ...
				if (!string.IsNullOrEmpty(message.ReplyTo.EMail))
					writer.Append("Reply-To: " + GetEMail(message.ReplyTo) + EndLine);
				// Remitente
				if (!string.IsNullOrEmpty(message.From.EMail))
					writer.Append("From: " + GetEMail(message.From) + EndLine);
				// Destinatarios
				writer.Append("To: " + CreateAddressList(message.To) + EndLine);
				if (message.CC.Count > 0)
					writer.Append("CC: " + CreateAddressList(message.CC) + EndLine);
				if (message.BCC.Count > 0)
					writer.Append("BCC: " + CreateAddressList(message.BCC) + EndLine);
				// Asunto
				if (!string.IsNullOrEmpty(message.Subject))
					writer.Append("Subject: " + LibEncoder.Encoder.Encode(LibEncoder.Encoder.EncoderType.QuotedPrintable,
																		  message.Subject, true) + EndLine);
				// Añade la fecha
				writer.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R") + EndLine);
				// Añade la cabecera de emisión
				writer.Append("X-Mailer: LibMailProtocols.net" + EndLine);
				// Añade los datos de notificación
				//if (notification)
				//{
				//  if (ReplyTo.name != null && ReplyTo.name.Length != 0)
				//           {
				//               sb.Append("Disposition-Notification-To: " + MailEncoder.ConvertHeaderToQP(ReplyTo.name, charset) + " <" + ReplyTo.address + ">\r\n");
				//           }
				//  else
				//  {
				//               sb.Append("Disposition-Notification-To: <" + ReplyTo.address + ">\r\n");
				//           }
				//}

				//if (Priority != MailPriority.Unknown)
				//  sb.Append("X-Priority: " + ((int) Priority).ToString() + "\r\n");
				// Añade las cabeceras
				writer.Append(GetHeaders(message));
				// Añade la versión MIME
				writer.Append("MIME-Version: 1.0" + EndLine);
				writer.Append(GetMessageBody(message, charSet));
				// Devuelve la cadena a enviar
				return writer.ToString();
		}

		/// <summary>
		///		Obtiene una dirección de correo
		/// </summary>
		private string GetEMail(Address address)
		{
			string eMail = $"<{address.EMail}>";

				// Añade el nombre
				if (!string.IsNullOrEmpty(address.Name))
					eMail = "\"" + EncodeQP(address.Name, false) + "\" " + eMail;
				// Devuelve la cadena
				return eMail;
		}

		/// <summary>
		///		Codifica un mensaje en QuotedPrintable
		/// </summary>
		private string EncodeQP(string value, bool isSubject)
		{
			return BauEncoder.Encoder.Encode(BauEncoder.Encoder.EncoderType.QuotedPrintable, value, isSubject);
		}

		/// <summary>
		///		Crea una lista de direcciones
		/// </summary>
		private string CreateAddressList(AddressesCollection addresses)
		{
			StringBuilder builder = new StringBuilder();

				// Añade las direcciones
				foreach (Address address in addresses)
					if (!string.IsNullOrEmpty(address.EMail))
					{
						// Añade una coma si es necesario
						if (builder.Length > 0)
							builder.Append(",");
						// Añade la dirección de correo
						builder.Append(GetEMail(address));
					}
				// Devuelve la lista de direcciones
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Obtiene las cabeceras de un mensaje
		/// </summary>
		private string GetHeaders(MimeMessage message)
		{
			StringBuilder headers = new StringBuilder();

				// Añade las cabeceras
				foreach (Header header in message.Headers)
					if (!string.IsNullOrEmpty(header.Name))
						headers.Append(header.Name + ":" + EncodeQP(header.Value, false) + EndLine);
				// Devuelve la cadena
				return headers.ToString().Trim();
		}

		/// <summary>
		///		Obtiene una cadena con el cuerpo del mensaje (incluyendo los adjuntos
		/// </summary>
		private string GetMessageBody(MimeMessage message, string charSet)
		{
			StringBuilder sb = new StringBuilder();
			string strMixedBoundary = GenerateMixedMimeBoundary();

				// Si hay adjuntos añade la cabecera
				if (message.Attachments.Count > 0)
				{
					sb.Append("Content-Type: multipart/mixed;" + EndLine);
					sb.Append(" boundary=\"" + strMixedBoundary + "\"");
					sb.Append("\r\n\r\nThis is a multi-part message in MIME format.");
					sb.Append("\r\n\r\n--" + strMixedBoundary + EndLine);
				}
				// Añade el cuerpo del mensaje
				sb.Append(GetInnerMessageBody(message, charSet));
				// Añade los adjuntos
				if (message.Attachments.Count > 0)
				{   
					//Añade el adjunto
					foreach (Section objAttachment in message.Attachments)
					{   
						// Separador
						sb.Append("\r\n--" + strMixedBoundary + EndLine);
						// Texto del adjunto
						sb.Append(GetAttachmentText(objAttachment, charSet));
					}
					// Añade el fin de mensaje
					sb.Append("\r\n--" + strMixedBoundary + "--\r\n");
				}
				// Devuelve la cadena
				return sb.ToString().Trim();
		}

		/// <summary>
		///		Obtiene el HTML y/o texto
		/// </summary>
		private string GetInnerMessageBody(MimeMessage message, string charSet)
		{
			string strRelatedBoundary = GenerateRelatedMimeBoundary();
			StringBuilder builder = new StringBuilder();

				// Añade la cabecera de las imágenes
				//if (images.Count > 0)
				//  {	sb.Append("Content-Type: multipart/related;");
				//    sb.Append("boundary=\"" + strRelatedBoundary + "\"");
				//    sb.Append("\r\n\r\n--" + strRelatedBoundary + "\r\n");
				//  }
				// Añade el cuerpo de mensaje (legible)
				builder.Append(GetReadableMessageBody(message, charSet));
				// Añade las imágenes
				//if (images.Count > 0)
				//  { // Añade las imágenes 
				//      foreach (Attachment image in images) 
				//        {	sb.Append("\r\n\r\n--" + strRelatedBoundary + "\r\n");
				//          sb.Append(image.ToMime());
				//        }
				//    // Añade el final de la sección
				//      sb.Append("\r\n\r\n--" + strRelatedBoundary + "--\r\n");
				//  }
				// Devuelve la cadena
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Obtiene la cadena con el cuerpo del mensaje
		/// </summary>
		private string GetReadableMessageBody(MimeMessage message, string charSet)
		{
			StringBuilder builder = new StringBuilder();

				// Añade el cuerpo del mensaje
				if (string.IsNullOrEmpty(message.BodyHTML.Content))
					builder.Append(GetTextMessageBody(message.Body.Content, "text/plain", charSet));
				else if (string.IsNullOrEmpty(message.Body.Content))
					builder.Append(GetTextMessageBody(message.BodyHTML.Content, "text/html", charSet));
				else
					builder.Append(GetAltMessageBody(GetTextMessageBody(message.Body.Content, "text/plain", charSet),
												GetTextMessageBody(message.BodyHTML.Content, "text/html", charSet)));
				// Devuelve la cadena
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Obtiene el texto de un cuerpo de mensaje
		/// </summary>
		private string GetTextMessageBody(string message, string textType, string charSet)
		{
			StringBuilder builder = new StringBuilder();

				// Cabecera
				builder.Append($"Content-Type: {textType};\r\n");
				builder.Append($" charset={charSet}\r\n");
				builder.Append("Content-Transfer-Encoding: quoted-printable\r\n\r\n");
				// Mensaje
				builder.Append(EncodeQP(message, false));
				// Devuelve la cadena
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Obtiene el cuerpo alternativo del mensaje
		/// </summary>
		private string GetAltMessageBody(string body, string html)
		{
			StringBuilder builder = new StringBuilder();
			string altBoundary = GenerateAltMimeBoundary();
			string relatedBoundary = GenerateRelatedMimeBoundary();

				// Añade la cabecera de las imágenes
				//if (images.Count > 0)
				{
					builder.Append("Content-Type: multipart/related;");
					builder.Append($"boundary=\"{relatedBoundary}\"");
					builder.Append($"\r\n\r\n--{relatedBoundary}\r\n");
				}
				// Cabecera
				builder.Append($"Content-Type: multipart/alternative;{EndLine}");
				builder.Append($" boundary=\"{altBoundary}\"");
				// sb.Append("\r\n\r\nThis is a multi-part message in MIME format.");
				builder.Append($"\r\n\r\n--{altBoundary}\r\n");
				// Cuerpo
				builder.Append(body);
				builder.Append($"\r\n\r\n--{altBoundary}\r\n");
				// Cuerpo HTML
				builder.Append(html);
				builder.Append($"\r\n\r\n--{altBoundary}--\r\n");

				// Añade las imágenes
				//if (images.Count > 0)
				//  { // Añade las imágenes 
				//      foreach (Attachment image in images) 
				//        {	sb.Append("\r\n\r\n--" + strRelatedBoundary + "\r\n");
				//          sb.Append(image.ToMime());
				//        }
				// Añade el final de la sección
				builder.Append($"\r\n\r\n--{relatedBoundary}--\r\n");
				//  }
				// Devuelve la cadena
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Añade el texto del adjunto
		/// </summary>
		private string GetAttachmentText(Section attachment, string charSet)
		{
			StringBuilder builder = new StringBuilder();

				//Content-Type: application/pdf; charset=iso-8859-1; name=N43QN93M.pdf
				//Content-Transfer-Encoding: base64
				//Content-Disposition: attachment; filename=N43QN93M.pdf
				// Cabecera		
				if (!string.IsNullOrEmpty(attachment.ID))
					builder.Append("Content-ID: <" + attachment.ID + ">\r\n");
				if (attachment.ContentDisposition.IsAttachment)
				{
					builder.Append("Content-Type: " + attachment.ContentDisposition.MediaType + ";");
					builder.Append("charset=" + charSet + ";\r\n");
					builder.Append(" name=\"" + EncodeQP(System.IO.Path.GetFileName(attachment.ContentDisposition.FileName), false) + "\"\r\n");
				}
				else
				{
					builder.Append("Content-Type: " + ContentType.GetContentType(ContentType.ContentTypeEnum.Base64) + ";\r\n");
					builder.Append(" name=\"" + EncodeQP(System.IO.Path.GetFileName(attachment.ContentDisposition.FileName), false) + "\"\r\n");
				}
				builder.Append("Content-Transfer-Encoding: " + ContentTransfer.GetTransferEncoding(ContentTransfer.ContentTransferEncoding.Base64) + "\r\n");
				builder.Append("Content-Disposition: attachment;\r\n");
				builder.Append(" filename=\"" + EncodeQP(System.IO.Path.GetFileName(attachment.ContentDisposition.FileName), false) +
											"\"\r\n\r\n");
				// Añade el archivo
				using (System.IO.FileStream fnFile = new System.IO.FileStream(attachment.ContentDisposition.FileName,
																			  System.IO.FileMode.Open, System.IO.FileAccess.Read))
				{
					byte [] buffer = new byte [fnFile.Length];

						// Lee el archivo
						fnFile.Read(buffer, 0, buffer.Length);
						// Añade el archivo codificado
						builder.Append(MakeLines(System.Convert.ToBase64String(buffer, 0, buffer.Length)));
						// Cierra el archivo
						fnFile.Close();
				}
				// Devuelve la cadena
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Separa el contenido en bloques de <see cref="MaxCharsPerLine"/> caracteres
		/// </summary>
		private string MakeLines(string source)
		{
			StringBuilder builder = new StringBuilder();
			int position = 0;

				// Recorre la cadena insertando los saltos de línea			
				for (int index = 0; index < source.Length; index++)
				{   
					// Añade el carácter
					builder.Append(source [index]);
					// Incrementa el contador de caracteres de la línea
					position++;
					// Añade un salto de línea si es necesario
					if (position >= MaxCharsPerLine)
					{ 
						// Añade el salto de línea
						builder.Append(EndLine);
						// Indica que es el primer carácter de la línea
						position = 0;
					}
				}
				// Devuelve la cadena con los saltos de línea
				return builder.ToString().Trim();
		}

		/// <summary>
		///		Genera una cadena de separación
		/// </summary>
		private string GenerateMixedMimeBoundary()
		{
			return "Part." + Convert.ToString(new Random(unchecked((int) DateTime.Now.Ticks)).Next()) + "." + Convert.ToString(new Random(~unchecked((int) DateTime.Now.Ticks)).Next());
		}

		/// <summary>
		///		Genera una cadena de separación
		/// </summary>
		private string GenerateAltMimeBoundary()
		{
			return "Part." + Convert.ToString(new Random(~unchecked((int) DateTime.Now.AddDays(1).Ticks)).Next()) + "." + Convert.ToString(new Random(unchecked((int) DateTime.Now.AddDays(1).Ticks)).Next());
		}

		/// <summary>
		///		Genera una cadena de separación
		/// </summary>
		private string GenerateRelatedMimeBoundary()
		{
			return "Part." + Convert.ToString(new Random(~unchecked((int) DateTime.Now.AddDays(2).Ticks)).Next()) + "." + Convert.ToString(new Random(unchecked((int) DateTime.Now.AddDays(1).Ticks)).Next());
		}
	}
}
