using System;
using System.IO;

namespace Bau.Libraries.LibEncoder.Encoders.Tools
{
	/// <summary>
	///		Archivo de ayuda para grabación
	/// </summary>
	internal static class FileHelper
	{
		/// <summary>
		///		Graba los datos en un archivo
		/// </summary>
		internal static void Save(string message, string fileName)
		{
			Save(System.Text.Encoding.UTF8.GetBytes(message), fileName);
		}

		/// <summary>
		///		Graba los datos en un archivo
		/// </summary>
		internal static void Save(byte[] source, string fileName)
		{
			using (FileStream fsOutput = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{ 
				// Escribe los bytes en el stream
				fsOutput.Write(source, 0, source.Length);
				// Cierra el stream de salida
				fsOutput.Close();
			}
		}
	}
}
