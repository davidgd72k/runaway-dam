using System.IO;
using UnityEngine;

public static class SaveSystem
{
    /// <summary>
    /// Carpeta donde se guardara el archivo de guardado.
    /// </summary>
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/saves/";
    /// <summary>
    /// Extensi�n del fichero de guardado.
    /// </summary>
    public static readonly string FILE_EXT = ".json";

    /// <summary>
    /// Guarda el JSON de los datos del juego en un fichero de texto.
    /// </summary>
    /// <param name="filename">Nombre del fichero donde se guardaran los cambios.</param>
    /// <param name="dataToSave">Contenido a guardar (se recomienda que sea contenido JSON).</param>
    public static void Save(string filename, string dataToSave)
    {
        // Aseguramos que exista la carpeta donde se guardaran los datos.
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        
        // Escribimos los datos en el fichero.
        File.WriteAllText(SAVE_FOLDER + filename + FILE_EXT, dataToSave);
    }

    /// <summary>
    /// Carga el JSON con los datos del juego desde el fichero de texto.
    /// </summary>
    /// <param name="filename">Nombre del fichero a cargar.</param>
    /// <returns>JSON con los datos del juego.</returns>
    public static string Load(string filename)
    {
        // Ruta completa del archivo
        string fileLoc = SAVE_FOLDER + filename + FILE_EXT;

        // Si existe, lo lee y lo devuelve
        if (File.Exists(fileLoc))
        {
            // Cargamos el fichero de guardado.
            string fileContent = File.ReadAllText(fileLoc);
            return fileContent;
        }
        else
        {
            // No se encontr� el fichero de guardado.
            return null;
        }
    }
}