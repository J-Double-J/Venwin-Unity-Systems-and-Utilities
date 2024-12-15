using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using File = System.IO.File;

namespace Venwin.Utilities
{
    public static class JsonFileUtil
    {
        /// <summary>
        /// Loads a json file and deserializes into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Object to be created from the file.</typeparam>
        /// <param name="fileName">File name to search for (without the extension).</param>
        /// <returns>The converted object from the JSON file.</returns>
        public static T LoadFromJson<T>(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");

            string jsonContent = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(jsonContent);
        }

        /// <summary>
        /// Exports an object to a JSON file.
        /// </summary>
        /// <param name="exportedObject">Object to convert.</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="overrwrite">If true overwrites the existing file, else appends a counter to the file name.</param>
        public static void ExportToJson(object exportedObject, string fileName, bool overrwrite = false)
        {
            // Serialize the object to JSON
            string json = JsonConvert.SerializeObject(exportedObject);

            string fileNameAndExtension = GetFileName(fileName, overrwrite);

            // Define file path
            string filePath = Path.Combine(Application.persistentDataPath, fileNameAndExtension);

            // Write to file  
            File.WriteAllText(filePath, json);

            Debug.Log($"Object state saved to {filePath}");
        }


        private static string GetFileName(string fileName, bool overrwrite)
        {
            if (overrwrite) { return $"{fileName}.json"; }

            // Create a suitable name for a file that shares the same name.
            int counter = 0;
            string fileNameAndExtension;

            do
            {
                fileNameAndExtension = counter == 0
                    ? $"{fileName}.json"
                    : $"{fileName} ({counter}).json";
                counter++;
            } while (File.Exists(fileName));

            return fileNameAndExtension;
        }
    }
}
