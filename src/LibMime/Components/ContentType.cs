using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Tipo de contenido
	/// </summary>
	public class ContentType
	{ 
		// Constantes internas con el tipo de contenido
		internal const string ContentTypeBase64 = "base64";
		internal const string ContentTypeHTML = "text/html";
		internal const string ContentTypeMultipart = "multipart";
		internal const string ContentTypeMultipartAlternative = "multipart/alternative";
		internal const string ContentTypeMultipartMixed = "multipart/mixed";
		internal const string ContentTypeMultipartRelated = "multipart/related";
		internal const string ContentTypeMultipartReport = "multipart/report";
		internal const string ContentTypeText = "text/plain";
		internal const string ContentTypeOctectStream = "application/octet-stream";
		// Tipos de contenido
		public enum ContentTypeEnum
		{
			Unknown,
			Base64,
			HTML,
			MultipartAlternative,
			MultipartMixed,
			MultipartRelated,
			MultipartReport,
			Multipart,
			OctectStream,
			Other,
			Text
		}
		// Variables privadas
		private string _contentType;

		/// <summary>
		///		Obtiene la descripción de un tipo de contenido
		/// </summary>
		internal static string GetContentType(ContentTypeEnum contentType)
		{
			switch (contentType)
			{
				case ContentTypeEnum.Base64:
					return ContentTypeBase64;
				case ContentTypeEnum.HTML:
					return ContentTypeHTML;
				case ContentTypeEnum.MultipartAlternative:
					return ContentTypeMultipartAlternative;
				case ContentTypeEnum.MultipartMixed:
					return ContentTypeMultipartMixed;
				case ContentTypeEnum.MultipartRelated:
					return ContentTypeMultipartRelated;
				case ContentTypeEnum.MultipartReport:
					return ContentTypeMultipartReport;
				case ContentTypeEnum.Multipart:
					return ContentTypeMultipart;
				case ContentTypeEnum.OctectStream:
					return ContentTypeOctectStream;
				case ContentTypeEnum.Text:
					return ContentTypeText;
				default:
					return "";
			}
		}

		/// <summary>
		///		Tipo de contenido
		/// </summary>
		public ContentTypeEnum Type { get; set; }

		/// <summary>
		///		Cadena con el tipo de contenido de la sección (sólo es necesario si es otros)
		/// </summary>
		public string ContentTypeDefinition
		{
			get
			{
				string contentTypeDefinition = GetContentType(Type);

					if (!string.IsNullOrEmpty(contentTypeDefinition))
						return contentTypeDefinition;
					else
						return _contentType;
			}
			set
			{ 
				// Asigna el tipo de contenido 
				_contentType = value;
				Type = ContentTypeEnum.Other;
				// Comprueba si es alguno de los tipos de contenido identificados
				if (!string.IsNullOrEmpty(_contentType))
				{ 
					// Limpia el tipo
					_contentType = _contentType.Trim();
					// Asigna el tipo
					if (_contentType.Equals(ContentTypeBase64, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.Base64;
					else if (_contentType.Equals(ContentTypeHTML, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.HTML;
					else if (_contentType.Equals(ContentTypeMultipartAlternative, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.MultipartAlternative;
					else if (_contentType.Equals(ContentTypeMultipartMixed, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.MultipartMixed;
					else if (_contentType.Equals(ContentTypeMultipartRelated, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.MultipartRelated;
					else if (_contentType.Equals(ContentTypeMultipartReport, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.MultipartReport;
					else if (_contentType.StartsWith(ContentTypeMultipart, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.Multipart;
					else if (_contentType.Equals(ContentTypeOctectStream, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.OctectStream;
					else if (_contentType.Equals(ContentTypeText, StringComparison.CurrentCultureIgnoreCase))
						Type = ContentTypeEnum.Text;
				}
			}
		}

		/// <summary>
		///		Conjunto de caracteres
		/// </summary>
		public string CharSet { get; set; }

		/// <summary>
		///		Boundary del mensaje
		/// </summary>
		public string Boundary { get; set; }

		/// <summary>
		///		Indica si es una sección múltiple
		/// </summary>
		public bool IsMultipart
		{
			get
			{
			return Type == ContentTypeEnum.Multipart || Type == ContentTypeEnum.MultipartAlternative ||
						   Type == ContentTypeEnum.MultipartMixed || Type == ContentTypeEnum.MultipartRelated;
			}
		}
	}
}
