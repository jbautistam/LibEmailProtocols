using System;
using System.IO;

namespace Bau.Libraries.LibMime.Services.Helpers
{
	/// <summary>
	///		Intérprete de líneas
	/// </summary>
	internal class ParserLines
	{ 
		// Variables privadas
		private StringReader _reader;
		private bool _isEof = false;

		/// <summary>
		///		Interpreta las líneas de un mensaje
		/// </summary>
		internal ParserLines(string message)
		{ 
			// Quita el punto final al mensaje
			if (message.EndsWith(".\r\n"))
				message = message.Substring(0, message.Length - 3);
			if (message.EndsWith(".\n"))
				message = message.Substring(0, message.Length - 2);
			else if (message.EndsWith("."))
				message = message.Substring(0, message.Length - 1);
			// Quita los espacios
			if (!string.IsNullOrEmpty(message))
				message = message.Trim();
			// Crea el lector
			_reader = new StringReader(message);
		}

		/// <summary>
		///		Lee una línea del buffer
		/// </summary>
		internal string ReadLine()
		{
			string line = _reader.ReadLine();

				// Comprueba si es fin de archivo		
				if (line == null)
					_isEof = true;
				// Devuelve la línea
				return line;
		}

		/// <summary>
		///		Lee las líneas concatenando con las siguientes
		/// </summary>
		internal string ReadLineContinuous()
		{
			string line = ReadLine();

				// Lee las siguientes líneas y las concatena
				if (line != "")
					while (_reader.Peek() == (int) ' ' || _reader.Peek() == (int) '\t')
					{
						string nextLine = ReadLine();

							if (!string.IsNullOrEmpty(nextLine))
								line += " " + nextLine.Trim();
					}
				// Devuelve las líneas concatenadas
				return line;
		}

		/// <summary>
		///		Indica si es un fin de archivo
		/// </summary>
		internal bool IsEof
		{
			get { return _reader == null || _isEof; }
		}
	}
}
