using System;
using System.Collections.Generic;
using System.Net.Mail;

using Bau.Libraries.LibMime.Components;

namespace Bau.Libraries.LibMailProtocols.Helper
{
	/// <summary>
	///		Clase de ayuda para envío de correos
	/// </summary>
	public class Mailer
	{
		public Mailer(string server, int port, string user, string password, bool useSSL)
		{
			Server = server;
			Port = port;
			User = user;
			Password = password;
			UseSSL = useSSL;
		}

		/// <summary>
		///		Concatena varias direcciones de correo sin repetición
		/// </summary>
		public static string ConcatEMail(string eMailSource, string newEMail)
		{
			return ConcatEMail(eMailSource, newEMail, false, null);
		}

		/// <summary>
		///		Concatena varias direcciones de correo sin repetición
		/// </summary>
		public static string ConcatEMail(string eMailSource, string newEMail, bool debug, string eMailDebug)
		{ 
			// Va añadiendo los correos a la cadena de correos
			if (!string.IsNullOrEmpty(newEMail))
			{
				string [] eMails = newEMail.Split(';');

					foreach (string emailTo in eMails)
						if (!string.IsNullOrEmpty(emailTo) &&
								eMailSource.IndexOf(emailTo, StringComparison.CurrentCultureIgnoreCase) < 0)
						{ 
							// Añade el separador
							if (!string.IsNullOrEmpty(eMailSource))
								eMailSource += ";";
							// Añade el correo
							if (debug)
								eMailSource += eMailDebug;
							else
								eMailSource += emailTo;
						}
			}
			// Devuelve la cadena con los correos
			return eMailSource;
		}

		/// <summary>
		///		Envía un correo
		/// </summary>
		public bool Send(string emailTo, string subject, string body)
		{
			return Send(emailTo, subject, body, null, new List<string>());
		}

		/// <summary>
		///		Envía un correo
		/// </summary>
		public bool Send(string emailTo, string subject, string body, string bodyHTML,
						 string fileAttachment)
		{
			return Send(emailTo, subject, body, bodyHTML, new List<string> { fileAttachment });
		}

		/// <summary>
		///		Envía un correo con una colección de adjuntos
		/// </summary>
		public bool Send(string emailTo, string subject, string body, string bodyHTML,
						 List<string> fileNames)
		{
			return Send(GetMessage(emailTo, subject, body, bodyHTML, fileNames));
		}

		/// <summary>
		///		Envía un correo con una colección de adjuntos y una excepción
		/// </summary>
		public bool Send(string emailTo, string subject, string body, string bodyHTML,
						 List<string> fileNames, Exception exception)
		{
			return Send(GetMessage(emailTo, subject, body + Environment.NewLine + GetMessageException(exception),
								   bodyHTML, fileNames));
		}

		/// <summary>
		///		Envía un correo con la excepción
		/// </summary>
		public bool Send(string addressTo, string subject, Exception exception)
		{
			return Send(addressTo, subject, "", exception);
		}

		/// <summary>
		///		Envía un correo con la excepción
		/// </summary>
		public bool Send(string addressTo, string subject, string body, Exception exception)
		{
			return Send(addressTo, subject, body + Environment.NewLine + GetMessageException(exception),
						null, new List<string>());
		}

		/// <summary>
		///		Obtiene el mensaje de una excepción
		/// </summary>
		private string GetMessageException(Exception exception)
		{
			string text = "Excepción desconocida";

				// Recoge la información de la excepción
				if (exception != null)
					text = $"Excepción: {exception.Message}\r\nTraza: {exception.StackTrace}";
				// Devuelve el texto de la excepción
				return text;
		}

		/// <summary>
		///		Envía un correo
		/// </summary>
		private bool Send(MailMessage message)
		{
			bool sent = false;

				// Envía el correo
				try
				{
					if (message.To != null && message.To.Count > 0)
					{
						SmtpClient smtp = new SmtpClient(Server, Port);

							// Asigna las credenciales
							smtp.Credentials = new System.Net.NetworkCredential(User, Password);
							smtp.EnableSsl = UseSSL;
							// Envía el correo
							smtp.Send(message);
							// Indica que se ha enviado correctamente
							sent = true;
					}
				} 
				catch (Exception exception)
				{
					System.Diagnostics.Debug.WriteLine(exception.Message);
					System.Diagnostics.Debug.WriteLine("Mensaje: " + message.Subject);
					System.Diagnostics.Debug.WriteLine(message.Body);
				}
				// Devuelve el valor que indica si se ha enviado el correo
				return sent;
		}

		/// <summary>
		///		Obtiene un mensaje MIME para enviarlo por eMail
		/// </summary>
		private MailMessage GetMessage(string addressTo, string subject, string body, string bodyHTML,
									   List<string> fileNames)
		{
			MailMessage message = new MailMessage();
			string [] addresses = addressTo.Split(';');

				// Añade los destinatarios
				foreach (string address in addresses)
					if (Address.CheckEMail(address))
						// if (message.To.Count == 0)
						message.To.Add(new MailAddress(address.Trim()));
					// else
					//	message.Bcc.Add(new MailAddress(address.Trim()));
				// Remitente del mensaje
				message.From = new MailAddress(User);
				// Asigna los datos del mensaje
				message.BodyEncoding = System.Text.Encoding.UTF8;
				message.SubjectEncoding = System.Text.Encoding.UTF8;
				if (!string.IsNullOrEmpty(subject))
					subject = subject.Trim();
				message.Subject = subject;
				message.Body = body;
				if (!string.IsNullOrEmpty(bodyHTML))
				{
					message.Body = bodyHTML;
					message.IsBodyHtml = true;
				}
				// Añade los adjuntos
				if (fileNames != null)
					foreach (string fileAttachment in fileNames)
						if (!string.IsNullOrEmpty(fileAttachment))
							message.Attachments.Add(new Attachment(fileAttachment));
				// Devuelve el mensaje
				return message;
		}

		/// <summary>
		///		Servidor SMTP
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		///		Puerto del servidor SMTP
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		///		Usuario del servidor SMTP
		/// </summary>
		public string User { get; set; }

		/// <summary>
		///		Contraseña del servidor SMTP
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		///		Indica si se debe utilizar SSL en las comunicaciones con el servidor SMTP
		/// </summary>
		public bool UseSSL { get; set; }
	}
}