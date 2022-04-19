using System;

namespace Bau.Libraries.LibMime.Components
{
	/// <summary>
	///		Dirección de correo
	/// </summary>
	public class Address
	{
		public Address() : this(null, null) { }

		public Address(string address)
		{
			Parse(address);
		}

		public Address(string name, string eMail)
		{
			Name = name;
			EMail = eMail;
		}

		/// <summary>
		///		Interpreta una dirección de correo 
		/// </summary>
		/// <example>
		///		Jose Antonio Bautista <jbautistam@gmail.com>
		/// </example>
		private void Parse(string address)
		{
			if (!string.IsNullOrEmpty(address))
			{
				int index = address.LastIndexOf('<');

					if (index < 0)
					{
						Name = null;
						EMail = address;
					} 
					else
					{
						int endIndex = address.LastIndexOf('>');

							Name = address.Substring(0, index);
							EMail = address.Substring(index + 1, endIndex - index - 1);
					}
			}
		}

		/// <summary>
		///		Comprueba una dirección de correo electrónico
		/// </summary>
		public static bool CheckEMail(string eMail)
		{
			bool isValid = false;

				// Comprueba si el correo es sintácticamente válido
				if (!string.IsNullOrEmpty(eMail))
				{
					string regex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
															   @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
															   @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
					System.Text.RegularExpressions.Regex expression = new System.Text.RegularExpressions.Regex(regex);

						// Comprueba si el correo es correcto
						isValid = expression.IsMatch(eMail);
				}
				// Devuelve el valor que indica si el correo es válido
				return isValid;
		}

		/// <summary>
		///		Nombre del usuario
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Dirección del correo
		/// </summary>
		public string EMail { get; set; }

		/// <summary>
		///		Dirección completa de correo electrónico
		/// </summary>
		public string FullEMail
		{
			get
			{
				if (string.IsNullOrEmpty(Name))
					return EMail;
				else
					return Name + " <" + EMail + ">";
			}
		}
	}
}
