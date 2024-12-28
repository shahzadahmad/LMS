using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Utilities
{
    public static class PasswordHasherUtility
    {
        private static readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public static string HashPassword(string password)
        {
            // The 'null' object is used as the user, since we don't have an IdentityUser instance here
            return _passwordHasher.HashPassword(null, password);
        }

        public static bool VerifyPasswordHash(string hashedPassword, string providedPassword)
        {
            try
            {
                // Attempt to convert the hashed password from Base64
                var hashBytes = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException ex)
            {
                // Log the error and rethrow or handle it as needed
                Console.WriteLine($"Error: {ex.Message}");
                throw new InvalidOperationException("The hashed password is not a valid Base64 string.", ex);
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);

            if (verificationResult == PasswordVerificationResult.Success || verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
