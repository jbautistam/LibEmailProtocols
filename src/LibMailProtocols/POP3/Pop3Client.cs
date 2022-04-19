using System;

namespace Bau.Libraries.LibMailProtocols.POP3
{
	/// <summary>
	///		Clase cliente para un protocolo Pop3 de recepción de correos electrónicos
	/// </summary>
	public class Pop3Client : IDisposable
	{   
		// Constantes privadas
		private const string OkCommand = "+ok ";
		private const string NewLine = "\r\n";
		// Eventos
		public event EventHandler<Pop3EventArgs> Pop3Action;
		// Variables privadas
		private Tools.Network.Connection _connection;

		public Pop3Client(string server, int port, string user, string password, bool useSsl)
							: this(new MailServer(server, port, user, password, useSsl)) { }

		public Pop3Client(MailServer server)
		{
			Server = server;
		}

		/// <summary>
		///		Conecta al servidor de correo
		/// </summary>
		public void Connect()
		{
			string response = "";

				// Desconecta
				Disconnect();
				// Lanza el evento de comienzo de conexión
				RaiseEvent(Pop3EventArgs.ActionType.ConnectionEstablishing);
				// Conecta al servidor
				_connection = new Tools.Network.Connection(Server);
				_connection.Connect();
				// Comprueba la respuesta del servidor
				response = _connection.Receive();
				if (string.IsNullOrEmpty(response) || 
						!response.StartsWith(OkCommand, StringComparison.OrdinalIgnoreCase))
					TreatError($"Server {Server.Address} did not send welcome message.");
				else
				{
					string digest = GetAuthenticateDigest(response);

						// Lanza el evento de conexión establecida
						RaiseEvent(Pop3EventArgs.ActionType.ConnectionEstablished);
						// Espera dos segundos
						System.Threading.Thread.Sleep(2000);
						// Lanza el evento de inicio de autentificación
						RaiseEvent(Pop3EventArgs.ActionType.AuthenticationBegan);
						// Envía usuario y contraseña
						Authenticate(digest);
						// Lanza el evento de final de autentificación
						RaiseEvent(Pop3EventArgs.ActionType.AuthenticationFinished);
				}
		}

		/// <summary>
		///		Obtiene la cadena para el digest MD5. La cadena de respuesta de inicio del servidor
		///	será del tipo +OK xxxxxxxxx &ltCadena digest&gt
		/// </summary>
		private string GetAuthenticateDigest(string response)
		{
			int startIndex = response.IndexOf("<");
			string digest = "";

				// Si hay una cadena Digest en la respuesta del servidor
				if (startIndex >= 0)
				{
					int indexEnd = response.IndexOf(">", startIndex);

						if (indexEnd >= 0)
						{
							string timeStamp = response.Substring(startIndex, indexEnd - startIndex + 1);

								// Envía la contraseña
								Send("PASSWORD " + Tools.Cryptography.MD5Helper.Compute(timeStamp + Server.Password), false);
						}
				}
				// Devuelve la cadena Digest
				return digest;
		}

		/// <summary>
		///		Autentifica el usuario
		/// </summary>
		private void Authenticate(string digest)
		{
			if (string.IsNullOrEmpty(digest)) // autentifica el usuario en plano
			{
				string response = Send("USER " + Server.User, false);

					// Comprueba si se le envía una cadena Digest como respuesta al comando USER	
					digest = GetAuthenticateDigest(response);
					// Envía la contraseña o el hash de contraseña
					if (string.IsNullOrEmpty(digest)) // ... simplemente se envía la contraseña en plano
						Send("PASS " + Server.Password, false);
					else // ... envía el MD5 de la contraseña
						Send("PASS " + Tools.Cryptography.MD5Helper.Compute(digest + Server.Password), false);
			} 
			else // ... autentifica el usuario utilizando APOP
				Send("APOP " + Server.User + " " + Tools.Cryptography.MD5Helper.Compute(digest + Server.Password), false);
		}

		/// <summary>
		///		Desconecta del servidor de correo
		/// </summary>
		public void Disconnect()
		{
			if (_connection != null)
			{   
				// Envía el comando
				Send("QUIT", false);
				// Desconecta del servidor
				ShutDown();
				// Lanza el evento de desconexión
				RaiseEvent(Pop3EventArgs.ActionType.Disconnected);
			}
		}

		/// <summary>
		///		Cierra la conexión física
		/// </summary>
		private void ShutDown()
		{
			if (_connection != null)
			{
				_connection.Disconnect();
				_connection = null;
			}
		}

		/// <summary>
		///		Obtiene los detalles de un buzón
		/// </summary>
		public void GetMailBoxDetails(out int numberEMails, out int totalSize)
		{
			string response = "";

				// Inicializa los argumentos de salida
				numberEMails = 0;
				totalSize = 0;
				// Envía el comando
				response = Send("STAT", false);
				// Obtiene los índices donde se encuentran los datos en la respuesta del servidor
				GetIndexSpaces(response, out int firstSpaceIndex, out int secondSpaceIndex, out int lineTerminationIndex);
				// Recoge los detalles del buzón
				numberEMails = ConvertToInt(response, firstSpaceIndex + 1, secondSpaceIndex);
				totalSize = ConvertToInt(response, secondSpaceIndex + 1, lineTerminationIndex);
		}

		/// <summary>
		///		Obtiene el tamaño de un correo
		/// </summary>
		public int GetMailSize(int messageIndex)
		{
			string response = "";

				// Envía el comando al servidor
				response = Send("LIST " + messageIndex, false);
				// Obtiene los índices donde se encuentran los datos en la respuesta del servidor
				GetIndexSpaces(response, out int firstSpaceIndex, out int secondSpaceIndex, out int lineTerminationIndex);
				// Devuelve el tamaño del buzón
				return ConvertToInt(response, secondSpaceIndex + 1, lineTerminationIndex);
		}

		/// <summary>
		///		Obtiene los índices donde se encuentran los espacios y la terminación de línea en una cadena
		/// </summary>
		private void GetIndexSpaces(string response, out int firstSpaceIndex, out int secondSpaceIndex,
									out int lineTerminationIndex)
		{ 
			// Obtiene los índices donde se encuentran los espacios
			firstSpaceIndex = response.IndexOf(" ", OkCommand.Length - 1);
			secondSpaceIndex = response.IndexOf(" ", firstSpaceIndex + 1);
			lineTerminationIndex = response.IndexOf(NewLine, secondSpaceIndex + 1);
			// Comprueba los errores
			if (firstSpaceIndex == -1 || secondSpaceIndex == -1 || lineTerminationIndex == -1)
				throw new Pop3ClientException("Server sent an invalid reply for command.");
		}

		/// <summary>
		///		Convierte una cadena en un entero
		/// </summary>
		private int ConvertToInt(string value, int start, int end)
		{
			if (!int.TryParse(value.Substring(start, end - start), out int result))
				return 0;
			else
				return result;
		}

		/// <summary>
		///		Borra un correo del servidor
		/// </summary>
		public void Delete(int messageIndex)
		{
			Send("DELE " + messageIndex, false);
		}

		/// <summary>
		///		Quita las marcas de borrado
		/// </summary>
		public void Undelete()
		{
			Send("RSET", false);
		}

		/// <summary>
		///		Recibe un correo
		/// </summary>
		public string FetchEmail(int messageIndex)
		{
			return FetchEmail(messageIndex, true);
		}

		/// <summary>
		///		Recibe la cabecera de un correo
		/// </summary>
		public string FetchEMailHeader(int messageIndex)
		{
			return FetchEmail(messageIndex, false);
		}

		/// <summary>
		///		Recibe un correo (completo o sólo la cabecera)
		/// </summary>
		private string FetchEmail(int messageIndex, bool readFull)
		{
			string response;
			int index;

				// Envía el comando al servidor	
				if (readFull) // ... recupera todo el correo
					response = Send("RETR " + messageIndex, true);
				else // ... recupera las primeras líneas del correo
					response = Send("TOP " + messageIndex + " " + 0, true);
				// Obtiene el contenido del correo
				index = response.IndexOf(NewLine);
				// Devuelve el correo interpretado
				return response.Substring(index + NewLine.Length, response.Length - index - NewLine.Length).Replace("\0", "");
		}

		/// <summary>
		///		Envía un mensaje y recibe la respuesta
		/// </summary>
		private string Send(string message, bool responseMultiline)
		{
			string response = "";
			bool waitDataAvailable = true;

				// Lanza el evento de envío de un comando
				RaiseEvent(Pop3EventArgs.ActionType.SendedCommand, message);
				// Envía el comando
				_connection.Send(message + NewLine);
				// Envía el comando y obtiene la respuesta
				do
				{ 
					// Obtiene la respuesta
					response += _connection.Receive(waitDataAvailable);
					// Indica que es (al menos) la segunda vez que se recibe parte de un mensaje
					waitDataAvailable = false;
				}
				while (!IsEndResponse(responseMultiline, response));
				// Lanza el evento de fin de un envío
				RaiseEvent(Pop3EventArgs.ActionType.ServerResponse, response);
				// Comprueba la respuesta del servidor
				if (string.IsNullOrEmpty(response) ||
						!response.StartsWith(OkCommand, StringComparison.OrdinalIgnoreCase))
					TreatError(response);
				// Devuelve la respuesta (si todo ha ido bien)
				return response;
		}

		/// <summary>
		///		Comprueba si se ha finalizado una respuesta
		/// </summary>
		private bool IsEndResponse(bool responseMultiline, string response)
		{
			return !responseMultiline ||
						   (responseMultiline &&
								  ((response.EndsWith("." + NewLine) ||
									  response.EndsWith("." + "\n")) &&
									   !response.EndsWith(NewLine + NewLine + ".")));
		}

		/// <summary>
		///		Trata un error: desconecta del servidor y lanza una excepción
		/// </summary>
		private void TreatError(string error)
		{ 
			// Desconecta
			ShutDown();
			// Lanza una excepción
			throw new Pop3ClientException(error);
		}

		/// <summary>
		///		Lanza un evento
		/// </summary>
		private void RaiseEvent(Pop3EventArgs.ActionType action)
		{
			RaiseEvent(action, null);
		}

		/// <summary>
		///		Lanza un evento
		/// </summary>
		private void RaiseEvent(Pop3EventArgs.ActionType action, string description)
		{
			Pop3Action?.Invoke(this, new Pop3EventArgs(action, Server, description));
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
			if (disposing && _connection != null)
				ShutDown();
		}

		/// <summary>
		///		Destructor del objeto
		/// </summary>
		~Pop3Client()
		{
			Dispose(false);
		}

		/// <summary>
		///		Servidor Pop3
		/// </summary>
		public MailServer Server { get; set; }
	}
}
