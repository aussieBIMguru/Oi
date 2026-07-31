using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Oi
{
    /// <summary>
    /// Provides methods for generating and validating authorization codes
    /// used to bypass deletion protection.
    /// </summary>
    public static class Authorization
    {
        internal static bool UserIsAdmin()
        {
            // Ensure settings directory exists
            if (!Directory.Exists(Globals.ADDIN_SETTINGS_FOLDER))
            {
                Directory.CreateDirectory(Globals.ADDIN_SETTINGS_FOLDER);
            }

            // If settings file is present...
            if (File.Exists(Globals.ADDIN_SETTINGS_FILE))
            {
                // Check its first row against the expected user name based bypass code
                string userCode = UtilFil.ReadFileAsRows(Globals.ADDIN_SETTINGS_FILE, true).FirstOrDefault();
                return userCode == BuildUserNameCode();
            }
            // Create a new file flagging as user (cosmetic only)
            else
            {
                var defaultData = new List<string>() { "USER", Globals.WINDOWS_USERNAME };
                UtilFil.WriteListToFile(Globals.ADDIN_SETTINGS_FILE, defaultData);
                return false;
            }
        }
        
        /// <summary>
        /// Secret key used when generating bypass codes.
        /// </summary>
        private static readonly byte[] Secret = Encoding.UTF8.GetBytes("OI");

        /// <summary>
        /// Builds a deterministic authorization code from a user name.
        /// If no user name is supplied, Windows username is used.
        /// </summary>
        /// <param name="userName">
        /// The user name to generate a code for, or <see langword="null"/> to use the
        /// current Windows username.
        /// </param>
        /// <returns>A short code representing the specified user name.</returns>
        internal static string BuildUserNameCode(string userName = null)
        {
            // Default is Revit user name
            userName ??= Globals.WINDOWS_USERNAME;

            // Hash the string and encode to a string
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(userName));
            return ToShortCode(hash, 15);
        }

        /// <summary>
        /// Builds a deterministic user-facing authorization code from a collection
        /// of element identifiers.
        /// </summary>
        /// <param name="ids">The element identifiers.</param>
        /// <returns>A short code representing the supplied element identifiers.</returns>
        internal static string BuildUserFacingCode(IEnumerable<ElementId> ids)
        {
            // Combine all ElementIds into a string
            List<long> sorted = ids.Select(i => i.Value).OrderBy(v => v).ToList();
            string joinedIdString = string.Join(",", sorted);

            // Hash the string and encode to a string
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(joinedIdString));
            return ToShortCode(hash, 15);
        }

        /// <summary>
        /// Generates the expected bypass code for a user-facing authorization code.
        /// </summary>
        /// <param name="userCode">The user-facing authorization code.</param>
        /// <returns>The corresponding bypass code.</returns>
        internal static string BuildActualBypassCode(string userCode)
        {
            // Using the secret as encryption
            using (var hmac = new HMACSHA256(Secret))
            {
                // Compute the equivalent response code
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userCode));
                return ToShortCode(hash, 15);
            }
        }

        /// <summary>
        /// Verifies that a supplied bypass code is valid for the specified
        /// user-facing authorization code.
        /// </summary>
        /// <param name="userCode">The user-facing authorization code.</param>
        /// <param name="userBypassCode">The bypass code to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the bypass code is valid; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal static bool VerifyUserBypassCode(string userCode, string userBypassCode)
        {
            // Early catch on null or empty
            if (userBypassCode.Ext_HasNoChars()) { return false; }

            // Get the required code
            string actualBypassCode = BuildActualBypassCode(userCode);

            // Check if they are matching excluding whitespace
            return string.Equals(
                actualBypassCode.Trim(),
                userBypassCode.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a hash into a shortened, URL-friendly uppercase code.
        /// </summary>
        /// <param name="hash">The hash to convert.</param>
        /// <param name="length">The number of characters to include in the result.</param>
        /// <returns>A shortened authorization code.</returns>
        private static string ToShortCode(byte[] hash, int length)
        {
            return Convert.ToBase64String(hash)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, length)
                .ToUpperInvariant();
        }
    }
}