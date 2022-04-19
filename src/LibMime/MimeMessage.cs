using System;

using Bau.Libraries.LibMime.Components;

namespace Bau.Libraries.LibMime
{
	/// <summary>
	///		Mensaje MIME
	/// </summary>
	public class MimeMessage
	{
		public MimeMessage()
		{
			ID = Guid.NewGuid().ToString();
			From = new Address();
			ReplyTo = new Address();
			To = new AddressesCollection();
			CC = new AddressesCollection();
			BCC = new AddressesCollection();
			Headers = new HeadersCollection();
			ContentType = new ContentType();
			TransferEncoding = new ContentTransfer();
			Body = new Section();
			BodyHTML = new Section();
			Attachments = new SectionsCollection();
		}

		/// <summary>
		///		ID del correo
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		///		Dirección de correo del remitente
		/// </summary>
		public Address From { get; set; }

		/// <summary>
		///		Dirección de respuesta
		/// </summary>
		public Address ReplyTo { get; set; }

		/// <summary>
		///		Destinatarios
		/// </summary>
		public AddressesCollection To { get; set; }

		/// <summary>
		///		Destinatarios de las copias
		/// </summary>
		public AddressesCollection CC { get; set; }

		/// <summary>
		///		Destinatarios ocultos de las copias
		/// </summary>
		public AddressesCollection BCC { get; set; }

		/// <summary>
		///		Asunto del mensaje
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		///		Versión de MIME
		/// </summary>
		public string MimeVersion { get; set; }

		/// <summary>
		///		Fecha
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		///		Tipo de contenido
		/// </summary>
		public ContentType ContentType { get; set; }

		/// <summary>
		///		Codificación de la transferencia
		/// </summary>
		public ContentTransfer TransferEncoding { get; set; }

		/// <summary>
		///		Juego de caracteres
		/// </summary>
		public string CharSet { get; set; }

		/// <summary>
		///		Cabeceras del mensaje de correo
		/// </summary>
		public HeadersCollection Headers { get; private set; }

		/// <summary>
		///		Cuerpo del mensaje
		/// </summary>
		public Section Body { get; internal set; }

		/// <summary>
		///		Cuerpo del mensaje en HTML
		/// </summary>
		public Section BodyHTML { get; internal set; }

		/// <summary>
		///		Adjuntos al mensaje
		/// </summary>
		public SectionsCollection Attachments { get; internal set; }
	}
}
