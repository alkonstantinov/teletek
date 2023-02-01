using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTeletekSW.Utils
{
    public static class FnDProcessors
    {

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, List<string> fileArgs)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            if (fileEntries.Length > 1)
                foreach (string fileName in fileEntries)
                    ProcessFile(fileName, fileArgs);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                if (subdirectory.EndsWith("Config") || subdirectory.EndsWith("Translation"))
                    ProcessDirectory(subdirectory, fileArgs);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string fileName, List<string> fileArgs)
        {
            if (fileName.Contains(".xml"))
            {
                fileArgs.Add(fileName);
            }
        }
    }
}
