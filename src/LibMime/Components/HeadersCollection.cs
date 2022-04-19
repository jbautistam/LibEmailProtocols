using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Colección e <see cref="Header"/>
	/// </summary>
	public class HeadersCollection : List<Header>
	{
		/// <summary>
		///		Añade una cabecera a la colección
		/// </summary>
		public void Add(string name, string value)
		{ 
			Add(new Header(name, value));
		}

		/// <summary>
		///		Busca una cabecera por su nombre
		/// </summary>
		public Header Search(string name)
		{ 
			// Recorre la colección buscando el elemento
			foreach (Header header in this)
				if (header.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
					return header;
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return null;
		}
		
		/// <summary>
		///		Comprueba si existe una cabecera por su nombre
		/// </summary>
		public bool Exists(string name)
		{ 
			return Search(name) != null;
		}
	}
}
