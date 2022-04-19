using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace Bau.Libraries.LibMailProtocols.Tools.Network
{
	/// <summary>
	///		Clase con las rutinas de conexión a la red mediante sockets
	/// </summary>
	internal class Connection : IDisposable
	{
		// Constantes privadas
		private const int BufferLength = 1024;
		private const int TimeOut = 60000;
		// Variables privadas
		private Socket _sckTcp;
		private NetworkStream _stmNetwork;
		private SslStream _stmSecure;
		private System.Text.ASCIIEncoding _encoding = new System.Text.ASCIIEncoding();

		internal Connection(MailServer server)
		{
			Server = server;
		}

		/// <summary>
		///		Conecta al servidor
		/// </summary>
		internal void Connect()
		{ 
			// Desconecta
			Disconnect();
			// Asigna el socket
			_sckTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_sckTcp.SendTimeout = TimeOut;
			_sckTcp.ReceiveTimeout = TimeOut;
			// Conecta el socket
			ConnectSocket(Server.Address, Server.Port);
			// Crea el stream de datos
			_stmNetwork = new NetworkStream(_sckTcp);
			// Crea el stream seguro si es necesario
			if (Server.UseSSL)
			{ 
				// Crea el stream de datos seguros
				_stmSecure = new SslStream(_stmNetwork, true);
				// Autentifica el cliente en una conexión segura
				_stmSecure.AuthenticateAsClient(Server.Address);
			}
		}

		/// <summary>
		///		Conecta el socket a un servidor
		/// </summary>
		private void ConnectSocket(string address, int port)
		{
			IPAddress [] ipAddresses;

				// Obtiene la dirección a partir de la DNS
				try
				{
					ipAddresses = Dns.GetHostAddresses(address);
				} 
				catch
				{
					throw new NetworkException($"Server \"{address}\" does not exist.");
				}
				// Conecta al punto remoto
				try
				{
					_sckTcp.Connect((EndPoint) new IPEndPoint(ipAddresses [0], port));
					_sckTcp.LingerState = new LingerOption(true, 20);
					_sckTcp.SendTimeout = TimeOut;
					_sckTcp.SendBufferSize = 8192;
					_sckTcp.ReceiveTimeout = TimeOut;
					_sckTcp.ReceiveBufferSize = 8192;
				} 
				catch (Exception exception)
				{
					throw new NetworkException($"Unable to connect to server: {address}, on port {port}", exception);
				}
		}

		/// <summary>
		///		Desconecta de la red
		/// </summary>
		internal void Disconnect()
		{ 
			// Cierra el stream de red
			if (_stmNetwork != null)
				_stmNetwork.Close((int) Server.Timeout.TotalSeconds);
			if (_stmSecure != null)
				_stmSecure.Close();
			// Cierra el socket
			if (_sckTcp != null && _sckTcp.Connected)
				_sckTcp.Close();
			// Libera la memoria
			_sckTcp = null;
			_stmNetwork = null;
			_stmSecure = null;
		}

		/// <summary>
		///		Envía un mensaje
		/// </summary>
		internal void Send(string message)
		{
			byte [] buffer = _encoding.GetBytes(message);

				if (Server.UseSSL)
					_stmSecure.Write(buffer, 0, buffer.Length);
				else
					_stmNetwork.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		///		Recibe un mensaje del servidor directamente (blnWitDataAvailable = false) o
		///	comprobando antes si se han recibido datos (evitando los bloqueos)
		/// </summary>
		internal string Receive(bool waitDataAvailable)
		{
			if (waitDataAvailable)
				return Receive();
			else
				return ReceiveLocking();
		}

		/// <summary>
		///		Recibe un mensaje del servidor
		/// </summary>
		internal string Receive()
		{
			DateTime start = DateTime.Now;

				// Espera hasta que haya datos disponibles
				while (!_stmNetwork.DataAvailable && !CheckTimeOut(start))
				{ 
					// Permite procesar los eventos
					System.Threading.Thread.Sleep(0);
					// Espera 1 segundo 
					System.Threading.Thread.Sleep(1000);
				}
				// Recoge los datos
				if (_stmNetwork.DataAvailable) // ... puede haber salido por el timeOut
					return ReceiveLocking();
				else
					throw new NetworkException("Connection timeout. Server unavailable");
		}

		/// <summary>
		///		Recibe un mensaje esperando los datos
		/// </summary>
		internal string ReceiveLocking()
		{
			System.Text.StringBuilder response = new System.Text.StringBuilder();
			byte [] buffer = new byte[BufferLength];

				// Lee los datos hasta que se acaba la transmisión
				do
				{
					int read = 0;

						// Lee los datos
						if (Server.UseSSL)
							read = _stmSecure.Read(buffer, 0, buffer.Length);
						else
							read = _stmNetwork.Read(buffer, 0, buffer.Length);
						// Concatena los datos
						response.Append(_encoding.GetString(buffer, 0, read));
				}
				while (_stmNetwork.DataAvailable);
				// Devuelve la cadena con la respuesta
				return response.ToString();
		}

		/// <summary>
		///		Comprueba si se ha sobrepasado el tiempo de espera
		/// </summary>
		private bool CheckTimeOut(DateTime start)
		{
			return (DateTime.Now - start).TotalMinutes >= Server.Timeout.TotalMinutes;
		}

		/// <summary>
		///		Obtiene la dirección del ordenador local
		/// </summary>
		internal static string GetLocalHostName()
		{
			return Dns.GetHostName();
		}

		/// <summary>
		///		Implementa el interface IDisposable
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Implementa el interface IDisposable
		/// </summary>
		private void Dispose(bool disposing)
		{
			if (disposing)
				Disconnect();
		}

		/// <summary>
		///		Destructor del objeto
		/// </summary>
		~Connection()
		{
			Dispose(false);
		}

		/// <summary>
		///		Servidor con el que se realiza la conexión
		/// </summary>
		internal MailServer Server { get; }
	}
}