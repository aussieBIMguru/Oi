// The class belongs to the Utilities namespace
using System.IO;

namespace Oi.Utilities
{
    /// <summary>
    /// Static methods container related to Files.
    /// </summary>
    public static class File_Utils
    {
        /// <summary>
        /// Runs an accessibility check on a file path.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns>A Boolean.</returns>
        public static bool FileIsAccessible(string filePath)
        {
            // If the file doesn't exist, we return true (to allow creation)
            if (!File.Exists(filePath))
            {
                return true;
            }

            // Try to open the file with exclusive access
            try
            {
                using (var stream = new FileStream(filePath,
                    FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // If we managed to run a stream, we can just return true
                    return true;
                }
            }
            // Otherwise the file was not accessible
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the contents of a file, by row.
        /// </summary>
        /// <param name="filePath">The file path to read.</param>
        /// <param name="skipEmpty">Do not write empty rows.</param>
        /// <returns>A list of strings.</returns>
        public static List<string> ReadFileAsRows(string filePath, bool skipEmpty = false)
        {
            // List of strings to return
            var rows = new List<string>();

            // Try to read the file...
            try
            {
                // Using a stream reader
                using (var reader = new StreamReader(filePath))
                {
                    // While we have more rows to read
                    while (!reader.EndOfStream)
                    {
                        // Read the line
                        string line = reader.ReadLine();

                        // If the row is not empty, add it
                        if (line.Ext_HasChars())
                        {
                            rows.Add(line);
                        }

                        // If it isn't, and we don't skip, add empty
                        else if (!skipEmpty)
                        {
                            rows.Add(string.Empty);
                        }
                    }
                }
            }
            // Report exception
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while trying to read the file: {ex.Message}");
            }

            // Return the list of strings (rows)
            return rows;
        }

        /// <summary>
        /// Writes a list of strings to a file.
        /// </summary>
        /// <param name="filePath">The file path to read.</param>
        /// <param name="dataRows">A list of strings to write.</param>
        /// <returns>A Result.</returns>
        public static Result WriteListToFile(string filePath, List<string> dataRows)
        {
            // Make sure file path is valid
            if (filePath is null || !FileIsAccessible(filePath))
            {
                return Result.Cancelled;
            }

            // Write to the file as list
            try
            {
                using (var writer = new StreamWriter(filePath, false))
                {
                    foreach (string row in dataRows)
                    {
                        writer.WriteLine(row);
                    }
                }
            }
            // Report exception
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while trying to write file: {ex.Message}");
                return Result.Cancelled;
            }

            // Return success
            return Result.Succeeded;
        }
    }
}