using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Seccion de un mensaje de correo
	/// </summary>
	public class Section
	{ 
		// Variables privadas
		private string _id;
			
		public Section()
		{ 
			Content = "";
		}
		
		/// <summary>
		///		Graba la sección
		/// </summary>
		public void Save(string fileName)
		{ 
			if (TransferEncoding.TransferEncoding == ContentTransfer.ContentTransferEncoding.Base64)
				LibEncoder.Encoder.DecodeToFile(LibEncoder.Encoder.EncoderType.Base64, Content, fileName);
			else if (TransferEncoding.TransferEncoding == ContentTransfer.ContentTransferEncoding.QuotedPrintable)
				LibEncoder.Encoder.DecodeToFile(LibEncoder.Encoder.EncoderType.QuotedPrintable, Content, fileName);
			else
				{ 
					using (System.IO.FileStream fsOutput = new System.IO.FileStream(fileName, System.IO.FileMode.CreateNew,
																					System.IO.FileAccess.Write,
																					System.IO.FileShare.None))
					{ 
						byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Content);
						
							// Escribe los bytes en el stream
							fsOutput.Write(buffer, 0, buffer.Length);
							// Cierra el stream de salida
							fsOutput.Close();																	
					}
				}
		}

		/// <summary>
		///		Identificador de la sección
		/// </summary>
		public string ID
		{ 
			get
			{ 
				// Si no se le ha asignado un ID se le añade una
				if (string.IsNullOrEmpty(_id))
					_id = Guid.NewGuid().ToString();
				// Devuelve el ID
				return _id;
			}
			set { _id = value; }
		}
		
		/// <summary>
		///		Tipo de contenido de la sección
		/// </summary>
		public ContentType ContentType { get; set; } = new ContentType();
	
		/// <summary>
		///		Disposición de la sección
		/// </summary>
		public ContentDisposition ContentDisposition { get; set; } = new ContentDisposition();
		
		/// <summary>
		///		Codificación
		/// </summary>
		public ContentTransfer TransferEncoding { get; set; } = new ContentTransfer();
		
		/// <summary>
		///		Contenido de la sección
		/// </summary>
		public string Content { get; set; }
		
		/// <summary>
		///		Cabeceras de la sección
		/// </summary>
		public HeadersCollection Headers { get; set; } = new HeadersCollection();
		
		/// <summary>
		///		Secciones de la sección
		/// </summary>
		public SectionsCollection Sections { get; set; } = new SectionsCollection();
	}
}
