using System;
using System.Security.Cryptography;
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Easy encryption and decryption of strings using Windows Data Protection API.
    /// </summary>
    /// <remarks>
    /// The code source is https://www.thomaslevesque.com/2013/05/21/an-easy-and-secure-way-to-store-a-password-using-data-protection-api/
    /// </remarks>
    internal static class DataProtector
    {
        public static string Protect(
            string clearText,
            string optionalEntropy = null,
            DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (clearText == null)
                throw new ArgumentNullException("clearText");
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, scope);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Unprotect(
            string encryptedText,
            string optionalEntropy = null,
            DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (encryptedText == null)
            {
                return string.Empty;
            }

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                    ? null
                    : Encoding.UTF8.GetBytes(optionalEntropy);
                byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, scope);
                return Encoding.UTF8.GetString(clearBytes);
            }
            catch
            {
                // If there was a problem unencrypt the data we ignore it
                // and return empty string.
                // This may happen when the stored data is not a valid encrypted data.
                return string.Empty;
            }
        }

    }
}
