using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Colección de secciones (<see cref="Section"/>
	/// </summary>
	public class SectionsCollection : System.Collections.Generic.List<Section>
	{
		/// <summary>
		///		Añade un nombre de archivo como adjunto
		/// </summary>
		public void Add(string fileName)
		{
			Section section = new Section();

				// Asigna las propiedades
				section.ContentType.Type = ContentType.ContentTypeEnum.Base64;
				section.TransferEncoding.TransferEncoding = ContentTransfer.ContentTransferEncoding.Base64;
				// Asigna la disposición de archivo
				section.ContentDisposition.Type = ContentDisposition.ContentDispositionEnum.Attachment;
				section.ContentDisposition.FileName = fileName;
				// Añade la sección
				Add(section);
		}

		/// <summary>
		///		Busca las secciones de determinado tipo
		/// </summary>
		public SectionsCollection Search(ContentType.ContentTypeEnum contentType)
		{
			SectionsCollection sections = new SectionsCollection();

				// Recorre las secciones buscando las de determinado tipo
				foreach (Section section in this)
				{ 
					// Comprueba si la sección es del tipo buscado
					if (section.ContentType.Type == contentType)
						sections.Add(section);
					// Añade las secciones hijas
					if (section.Sections.Count > 0)
						sections.AddRange(section.Sections.Search(contentType));
				}
				// Devuelve las secciones
				return sections;
		}

		/// <summary>
		///		Busca una sección a partir de su ID
		/// </summary>
		public Section Search(string id)
		{ 
			// Busca la sección
			foreach (Section section in this)
				if (section.ID.Equals(id, StringComparison.CurrentCultureIgnoreCase))
					return section;
			// Si ha llegado hasta aquí es porque no han encontrado nada
			return null;
		}
	}
}
