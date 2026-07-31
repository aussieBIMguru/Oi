// The class belongs to the Utilities namespace
namespace Oi.Utilities
{
    /// <summary>
    /// Static methods container related to Data containers.
    /// </summary>
    public static class Data_Utils
    {
        /// <summary>
        /// Returns a value based on a key from a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary of keys/values to search.</param>
        /// <param name="key">The key to search for.</param>
        /// <param name="defaultValue">The value to return if no key is found.</param>
        /// <returns>The related tooltip, if found.</returns>
        public static string GetDictValue(Dictionary<string, string> dictionary, string key, string defaultValue = "Value not found.")
        {
            if (dictionary.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}