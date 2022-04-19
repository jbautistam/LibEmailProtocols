using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Clase con los datos de transferencia del contenido
	/// </summary>
	public class ContentTransfer
	{   // Constantes internas con la codificación de la transferencia
		internal const string TransferEncodingBase64 = "base64";
		internal const string TransferEncodingQuotedPrintable = "quoted-printable";
		internal const string TransferEncoding8Bit = "8Bit";
		internal const string TransferEncoding7Bit = "7Bit";
		// Codificación del contenido para transferirlo
		public enum ContentTransferEncoding
		{
			Unknown,
			QuotedPrintable,
			Base64,
			Bit8,
			Bit7
		}
		// Variables privadas
		private string _transferEncoding;

		/// <summary>
		///		Obtiene la descripción del tipo de transferencia
		/// </summary>
		internal static string GetTransferEncoding(ContentTransferEncoding transferEncoding)
		{
			switch (transferEncoding)
			{
				case ContentTransferEncoding.Base64:
					return TransferEncodingBase64;
				case ContentTransferEncoding.QuotedPrintable:
					return TransferEncodingQuotedPrintable;
				case ContentTransferEncoding.Bit7:
					return TransferEncoding7Bit;
				case ContentTransferEncoding.Bit8:
					return TransferEncoding8Bit;
				default:
					return "";
			}
		}

		/// <summary>
		///		Codificación de la transferenca
		/// </summary>
		public ContentTransferEncoding TransferEncoding { get; set; }

		/// <summary>
		///		Definición de la codificación del contenido
		/// </summary>
		public string TransferEncodingDefinition
		{
			get
			{
				string transfer = GetTransferEncoding(TransferEncoding);

					if (!string.IsNullOrEmpty(transfer))
						return transfer;
					else
						return _transferEncoding;
			}
			set
			{ 
				// Asigna el tipo de transferencia
				_transferEncoding = value;
				TransferEncoding = ContentTransferEncoding.Unknown;
				// Comprueba si es alguno de los tipos de contenido identificados
				if (!string.IsNullOrEmpty(_transferEncoding))
				{ 
					// Quita los espacios
					_transferEncoding = _transferEncoding.Trim();
					// Asigna el tipo
					if (_transferEncoding.Equals(TransferEncodingBase64, StringComparison.CurrentCultureIgnoreCase))
						TransferEncoding = ContentTransferEncoding.Base64;
					else if (_transferEncoding.Equals(TransferEncodingQuotedPrintable, StringComparison.CurrentCultureIgnoreCase))
						TransferEncoding = ContentTransferEncoding.QuotedPrintable;
					else if (_transferEncoding.Equals(TransferEncoding8Bit, StringComparison.CurrentCultureIgnoreCase))
						TransferEncoding = ContentTransferEncoding.Bit8;
					else if (_transferEncoding.Equals(TransferEncoding8Bit, StringComparison.CurrentCultureIgnoreCase))
						TransferEncoding = ContentTransferEncoding.Bit7;
				}
			}
		}
	}
}
