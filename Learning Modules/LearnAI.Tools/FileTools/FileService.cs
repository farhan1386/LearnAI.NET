using System;
using System.ComponentModel;
using System.IO;

namespace LearnAI.Tools.FileTools
{
    public class FileService
    {
        [Description("Reads and extracts flat UTF-8 text from a target storage path file location.")]
        public string ReadProjectFile(
            [Description("The relative target storage filepath location string.")] string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            return File.ReadAllText(path);
        }

        [Description("Writes or appends raw text strings down into a target workspace file layout.")]
        public string WriteProjectFile(
            [Description("The targeted storage destination file system layout path.")] string path,
            [Description("The clear string body content payload to write out to storage.")] string content)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            File.WriteAllText(path, content);
            return $"Successfully committed text data onto location path target reference.";
        }
    }
}