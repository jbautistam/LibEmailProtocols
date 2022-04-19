using System;

using Bau.Libraries.LibEncoder;
using Bau.Libraries.LibMime;
using Bau.Libraries.LibMime.Components;
using Bau.Libraries.LibMime.Services;

namespace Bau.Libraries.LibMailProtocols.SMTP
{
	/// <summary>
	///		Clase cliente para un protocolo SMTP de recepci�n de correos electr�nicos
	/// </summary>
	public class SMTPClient
	{
		// Constantes privadas
		private const string OkCommand = "+ok ";
		private const string NewLine = "\r\n";
		private const string EndTrasmission = NewLine + "." + NewLine;
		// Eventos
		public event EventHandler<SMTPEventArgs> SMTPAction;
		// Variables privadas
		private Tools.Network.Connection _connection;

		public SMTPClient(string server, int port, string user, string password, bool useSsl = false)
							: this(new MailServer(server, port, user, password, useSsl)) { }

		public SMTPClient(MailServer server)
		{
			Server = server;
		}

		/// <summary>
		///		Conecta al servidor de correo
		/// </summary>
		public bool Connect()
		{
			SMTPResponse response = null;
			bool connected = false;

				// Desconecta
				Disconnect();
				// Lanza el evento de comienzo de conexi�n
				RaiseEvent(SMTPEventArgs.ActionType.ConnectionEstablishing);
				// Conecta al servidor
				_connection = new Tools.Network.Connection(Server);
				_connection.Connect();
				// Comprueba la respuesta del servidor
				response = new SMTPResponse(_connection.Receive());
				if (!response.IsOk)
					TreatError("Server " + Server.Address + " did not send welcome message.");
				else
				{ 
					// Manda un comando EHLO para comprobar si el servidor responde a comandos extendidos
					response = Send("EHLO " + Tools.Network.Connection.GetLocalHostName(), false);
					// Manda un comando HELO 
					if (!response.IsOk)
						response = Send("HELO " + Tools.Network.Connection.GetLocalHostName(), false);
					// Si es correcto, se autentifica
					if (response.IsOk)
					{ 
						// Lanza el evento de conexi�n establecida
						RaiseEvent(SMTPEventArgs.ActionType.ConnectionEstablished);
						// Lanza el comando de autentificaci�n
						if (Authenticate(Server.User, Server.Password))
							connected = true;
						else
							TreatError("Not aunthentified");
					}
				}
				// Devuelve el valor que indica si se ha conectado
				return connected;
		}

		/// <summary>
		///		Autentifica un usuario
		/// </summary>
		private bool Authenticate(string user, string password)
		{
			string encoded = Encoder.Encode(Encoder.EncoderType.Base64, ((char) 0) + user + ((char) 0) + password);
			SMTPResponse response = Send("AUTH PLAIN " + encoded, false);

				// Comprueba si se ha autentificado
				return response.IsOk;
		}

		/// <summary>
		///		Env�a un mensaje
		/// </summary>
		public void Send(MimeMessage message)
		{
			SMTPResponse response;

				// Env�a el FROM
				response = Send("MAIL FROM: " + GetEMailAddress(message.From), false);
				// Env�a el destinatario
				if (response.IsOk)
				{ 
					// Env�a los destinatarios
					SendRecipients(message.To);
					SendRecipients(message.CC);
					SendRecipients(message.BCC);
					// Env�a el mensaje
					SendMessage(message);
				} 
				else
					TreatError("Error en el env�o (comando MAIL FROM)");
		}

		/// <summary>
		///		Env�a los destinatarios
		/// </summary>
		private void SendRecipients(AddressesCollection addresses)
		{
			foreach (Address address in addresses)
			{
				SMTPResponse response = Send("RCPT TO: " + GetEMailAddress(address), false);

					if (!response.IsOk)
						TreatError("Error al enviar el destinatario " + address.EMail);
			}
		}

		/// <summary>
		///		Obtiene la direcci�n de correo con formato SMTP
		/// </summary>
		private string GetEMailAddress(Address address)
		{
			return $"<{address.EMail}>";
		}

		/// <summary>
		///		Env�a el mensaje
		/// </summary>
		private void SendMessage(MimeMessage message)
		{
			SMTPResponse response;

				// Comienza el env�o de datos
				response = Send("DATA", false);
				// Env�a los datos
				if (response.Code == 354)
				{ 
					// Env�a todos los datos del mensaje (MIME)
					response = Send(new MimeMessageCreator().CreateMessage(message) + EndTrasmission, false);
					// Comprueba si todo es correcto
					if (!response.IsOk)
						TreatError("Error en el env�o de datos");
				} 
				else
					TreatError("Error en el env�o (comando DATA)");
		}

		/// <summary>
		///		Desconecta del servidor de correo
		/// </summary>
		public void Disconnect()
		{
			if (_connection != null)
			{   
				// Env�a el comando
				Send("QUIT", false);
				// Desconecta del servidor
				ShutDown();
				// Lanza el evento de desconexi�n
				RaiseEvent(SMTPEventArgs.ActionType.Disconnected);
			}
		}

		/// <summary>
		///		Cierra la conexi�n f�sica
		/// </summary>
		private void ShutDown()
		{
			_connection.Disconnect();
			_connection = null;
		}

		/// <summary>
		///		Env�a un mensaje y recibe la respuesta
		/// </summary>
		private SMTPResponse Send(string message, bool responseMultiline)
		{
			string response = "";
			bool waitDataAvailable = true;

				// Lanza el evento de env�o de un comando
				RaiseEvent(SMTPEventArgs.ActionType.SendedCommand, message);
				// Env�a el comando
				_connection.Send(message + NewLine);
				// Env�a el comando y obtiene la respuesta
				do
				{ 
					// Obtiene la respuesta
					response += _connection.Receive(waitDataAvailable);
					// Indica que es (al menos) la segunda vez que se recibe parte de un mensaje
					waitDataAvailable = false;
				}
				while (!IsEndResponse(responseMultiline, response));
				// Lanza el evento de fin de un env�o
				RaiseEvent(SMTPEventArgs.ActionType.ServerResponse, response);
				// Comprueba la respuesta del servidor
				return new SMTPResponse(response);
		}

		/// <summary>
		///		Comprueba si se ha finalizado una respuesta
		/// </summary>
		private bool IsEndResponse(bool responseMultiline, string response)
		{
			return !responseMultiline ||
						   (responseMultiline &&
								  (response.EndsWith("." + NewLine) &&
								   !response.EndsWith(NewLine + NewLine + "." + NewLine)));
		}

		/// <summary>
		///		Trata un error: desconecta del servidor y lanza una excepci�n
		/// </summary>
		private void TreatError(string error)
		{ 
			// Desconecta
			ShutDown();
			// Lanza una excepci�n
			throw new SMTPClientException(error);
		}

		/// <summary>
		///		Lanza un evento
		/// </summary>
		private void RaiseEvent(SMTPEventArgs.ActionType action)
		{
			RaiseEvent(action, null);
		}

		/// <summary>
		///		Lanza un evento
		/// </summary>
		private void RaiseEvent(SMTPEventArgs.ActionType action, string description)
		{
			SMTPAction?.Invoke(this, new SMTPEventArgs(action, Server, description));
		}

		/// <summary>
		///		Servidor SMTP
		/// </summary>
		public MailServer Server { get; set; }
	}
}
