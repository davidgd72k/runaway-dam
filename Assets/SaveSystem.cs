using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/saves/";
    public static readonly string FILE_EXT = ".json";

    public static void Save(string filename, string dataToSave)
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        
        File.WriteAllText(SAVE_FOLDER + filename + FILE_EXT, dataToSave);
    }

    public static string Load(string filename)
    {
        string fileLoc = SAVE_FOLDER + filename + FILE_EXT;
        
        if (File.Exists(fileLoc))
        {
            string fileContent = File.ReadAllText(fileLoc);
            return fileContent;
        }
        else
        {
            return null;
        }
    }
}
