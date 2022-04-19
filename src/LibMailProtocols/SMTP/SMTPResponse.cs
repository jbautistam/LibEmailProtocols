using System;

namespace Bau.Libraries.LibMailProtocols.SMTP
{
	/// <summary>
	///		Clase con los datos de una respuesta de un servidor SMTP
	/// </summary>
	internal class SMTPResponse
	{
		internal SMTPResponse(string message)
		{
			Parse(message);
		}

		/// <summary>
		///		Interpreta el mensaje
		/// </summary>
		internal void Parse(string message)
		{
			if (!string.IsNullOrEmpty(message) && message.Length > 4)
			{
				if (int.TryParse(message.Substring(0, 3), out int code))
				{ 
					// Se ha interpretado correctamente. Se guarda el código y el mensaje
					Code = code;
					Message = message.Substring(4);
				} 
				else
				{
					Code = -1;
					Message = message;
				}
			}
		}

		/// <summary>
		///		Código de la respuesta
		/// </summary>
		internal int Code { get; set; }

		/// <summary>
		///		Mensaje de la respuesta
		/// </summary>
		internal string Message { get; set; }

		/// <summary>
		///		Indica si el mensaje es una respuesta correcta
		/// </summary>
		internal bool IsOk
		{
			get { return Code >= 200 && Code <= 299; }
		}

		/// <summary>
		///		Indica si el mensaje es una respuesta errónea
		/// </summary>
		internal bool IsFatalError
		{
			get { return Code >= 500 && Code <= 599; }
		}
	}
}
