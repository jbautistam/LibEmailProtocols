using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Colección de <see cref="Address"/>
	/// </summary>
	public class AddressesCollection : List<Address>
	{
		public AddressesCollection() { }

		public AddressesCollection(string addresses)
		{
			Add(addresses);
		}

		/// <summary>
		///		Añade una dirección a la colección
		/// </summary>
		public void Add(string name, string eMail)
		{
			Add(new Address(name, eMail));
		}

		/// <summary>
		///		Añade una serie de direcciones separadas por comas
		/// </summary>
		/// <example>
		/// jbautistamontejo@gmail.com, jbautistam@ant2e6.site11.com, Jose Antonio Bautista &lt;jbautistam@gmail.com&gt;
		/// </example>
		public void Add(string addresses)
		{
			if (!string.IsNullOrEmpty(addresses))
			{
				string[] arraddress = addresses.Split(',');

					foreach (string address in arraddress)
						if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(address.Trim()))
							Add(new Address(address.Trim()));
			}
		}

		/// <summary>
		///		Cadena con las direcciones
		/// </summary>
		public string FullEMail
		{
			get
			{
				string eMail = "";

					// Añade las direcciones
					foreach (Address address in this)
					{ 
						// Añade el separador
						if (!string.IsNullOrEmpty(eMail))
							eMail += ", ";
						// Añade la dirección
						eMail += address.FullEMail;
					}
					// Devuelve las direcciones
					return eMail;
			}
		}
	}
}
