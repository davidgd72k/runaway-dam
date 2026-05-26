using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // Carpeta donde se guardan los archivos de guardado
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/saves/";

    // Extensión del archivo de guardado
    public static readonly string FILE_EXT = ".json";

    public static void Save(string filename, string dataToSave)
    {
        // Si la carpeta no existe, la crea
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }

        // Escribe el archivo en disco
        File.WriteAllText(SAVE_FOLDER + filename + FILE_EXT, dataToSave);
    }

    public static string Load(string filename)
    {
        // Ruta completa del archivo
        string fileLoc = SAVE_FOLDER + filename + FILE_EXT;

        // Si existe, lo lee y lo devuelve
        if (File.Exists(fileLoc))
        {
            string fileContent = File.ReadAllText(fileLoc);
            return fileContent;
        }
        else
        {
            // Si no existe, devuelve null
            return null;
        }
    }
}