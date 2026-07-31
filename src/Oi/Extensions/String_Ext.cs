using System.Globalization;

// The class belongs to the extensions namespace
namespace Oi.Extensions
{
    /// <summary>
    /// Extension methods to the String class.
    /// </summary>
    public static class String_Ext
    {
        /// <summary>
        /// Returns if a string has characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        public static bool Ext_HasChars(this string str)
        {
            return str?.Length > 0;
        }

        /// <summary>
        /// Returns if a string has no characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        public static bool Ext_HasNoChars(this string str)
        {
            return !str.Ext_HasChars();
        }

        /// <summary>
        /// If string is null, substitutes it.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="ifNull">Value to replace if null (optional).</param>
        /// <param name="replaceEmpty">Catch empty string case also.</param>
        /// <returns>A string.</returns>
        public static string Ext_DeNull(this string str, string ifNull = "", bool replaceEmpty = false)
        {
            if (replaceEmpty)
            {
                return str.Ext_HasChars() ? str : ifNull;
            }
            else
            {
                return str ?? ifNull;
            }
        }

        /// <summary>
        /// Sends the string to the clipboard.
        /// </summary>
        /// <param name="text">The text to send.</param>
        /// <returns>A Result.</returns>
        [STAThread]
        internal static Result Ext_SendToClipboard(this string text)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Result.Cancelled;
            }
        }

        /// <summary>
        /// Convert a string to a nullable integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <returns>A nullable integer.</returns>
        public static int? Ext_ToIntOrNull(this string text)
        {
            if (text.Ext_HasChars() &&
                int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int i))
            {
                return i;
            }

            return null;
        }

        /// <summary>
        /// Convert a string to an integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>An integer.</returns>
        public static int Ext_ToIntWithFallback(this string text, int failValue = 0)
        {
            return text.Ext_ToIntOrNull() ?? failValue;
        }

        /// <summary>
        /// Convert a string to a nullable double.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <returns>A nullable double.</returns>
        public static double? Ext_ToDoubleOrNull(this string text)
        {
            if (text.Ext_HasChars() &&
                double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
            {
                return d;
            }

            return null;
        }

        /// <summary>
        /// Convert a string to a double.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>A double.</returns>
        public static double Ext_ToDoubleOrFallback(this string text, double failValue = 0.0)
        {
            return text.Ext_ToDoubleOrNull() ?? failValue;
        }
    }
}
