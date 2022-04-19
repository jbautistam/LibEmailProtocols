using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Datos de una cabecera de correo
	/// </summary>
	public class Header
	{ 
		// Constantes con los nombres básicos de las cabeceras
		internal const string Undefined = "Undefined";
		internal const string MessageID = "Message-id";
		internal const string ContentType = "Content-Type";
		internal const string From = "From";
		internal const string To = "To";
		internal const string CC = "cc";
		internal const string Subject = "subject";
		internal const string Date = "date";
		internal const string MimeVersion = "mime-version";
		internal const string ContentTransferEncoding = "Content-Transfer-Encoding";
		internal const string ContentDisposition = "Content-Disposition";
		internal const string IDAttachment = "X-Attachment-Id";
		internal const string ContentTypeBoundary = "boundary";
		internal const string CharSet = "charset";
		internal const string FileName = "filename";

		public Header() : this(null, null) { }

		public Header(string name, string value)
		{
			Name = string.IsNullOrEmpty(name) ? "" : name.Trim();
			Value = string.IsNullOrEmpty(value) ? "" : value.Trim();
			SubHeaders = new HeadersCollection();
		}

		/// <summary>
		///		Busca el valor de una cabecera / subcabecera
		/// </summary>
		internal string SearchValue(string name)
		{
			string value = "";

				// Obtiene el valor		
				if (Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
					value = Value;
				else
				{
					Header header = SubHeaders.Search(name);

						if (header != null)
							value = header.Value;
				}
				// Devuelve el valor encontrado
				return value;
		}

		/// <summary>
		///		Nombre de la cabecera
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Valor de la cabecera
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		///		Subcabeceras
		/// </summary>
		public HeadersCollection SubHeaders { get; set; }
	}
}
