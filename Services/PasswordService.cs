
using Microsoft.AspNetCore.Identity;

namespace intellectualconversationAPI.Services
{
    public sealed class PasswordService
    {
        private readonly PasswordHasher<string> _hasher = new();

        public string HashPassword(string password)
        {
            return _hasher.HashPassword("member", password);
        }

        public bool VerifyPassword(
            string password,
            string storedPassword,
            out bool needsRehash)
        {
            needsRehash = false;

            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            // First, check whether the stored value is an ASP.NET
            // Core Identity password hash.
            try
            {
                var result = _hasher.VerifyHashedPassword(
                    "member",
                    storedPassword,
                    password);

                if (result == PasswordVerificationResult.Success)
                {
                    return true;
                }

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    needsRehash = true;
                    return true;
                }
            }
            catch (FormatException)
            {
                // The stored value may be a legacy plaintext password.
            }
            catch (ArgumentException)
            {
                // Handle malformed stored hash as a possible legacy value.
            }

            // TEMPORARY: migrate existing plaintext passwords at login.
            // Remove this fallback after all legacy accounts are migrated.
            if (string.Equals(
                password,
                storedPassword,
                StringComparison.Ordinal))
            {
                needsRehash = true;
                return true;
            }

            return false;
        }
    }
}
