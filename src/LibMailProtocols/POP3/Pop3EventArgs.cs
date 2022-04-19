using System;

namespace Bau.Libraries.LibMailProtocols.POP3
{
	/// <summary>
	///		Argumentos de los eventos
	/// </summary>
	public class Pop3EventArgs : EventArgs
	{   
		// Enumerados
		public enum ActionType
		{
			Unknown,
			ConnectionEstablishing,
			ConnectionEstablished,
			AuthenticationBegan,
			AuthenticationFinished,
			Disconnected,
			SendedCommand,
			ServerResponse
		}

		public Pop3EventArgs(ActionType action, MailServer server, string description)
		{
			Action = action;
			Server = server;
			Description = description;
		}

		/// <summary>
		///		Acción realizada con el servidor
		/// </summary>
		public ActionType Action { get; }

		/// <summary>
		///		Servidor del que se envía o recibe un mensaje
		/// </summary>
		public MailServer Server { get; }

		/// <summary>
		///		Descripción del evento
		/// </summary>
		public string Description { get; }
	}
}
