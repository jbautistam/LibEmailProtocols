using System;

namespace Bau.Libraries.LibMailProtocols
{
	/// <summary>
	///		Clase con los datos de un servidor
	/// </summary>
	public class MailServer
	{
		public MailServer() : this(null, 110, null, null, false) { }

		public MailServer(string address, int port, string user, string password,
						  bool useSsl = false, TimeSpan? timeout = null)
		{
			Address = address;
			Port = port;
			UseSSL = useSsl;
			User = user;
			Password = password;
			Timeout = timeout ?? TimeSpan.FromSeconds(60);
		}

		/// <summary>
		///		Dirección o nombre del servidor
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		///		Puerto del servidor
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		///		Usuario del servidor
		/// </summary>
		public string User { get; set; }

		/// <summary>
		///		Contraseña del usuario
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		///		Indica si debe utilizar conexiones seguras
		/// </summary>
		public bool UseSSL { get; set; }

		/// <summary>
		///		Tiempo de espera máximo (en segundos)
		/// </summary>
		public TimeSpan Timeout { get; set; }
	}
}
