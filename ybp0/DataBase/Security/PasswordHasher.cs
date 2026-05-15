using System;
using System.Globalization;
using System.Security.Cryptography;

namespace DataBase.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static PasswordHash Create(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = DeriveKey(password, salt);
            return new PasswordHash(Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool Verify(string password, string storedHash, string storedSalt, out bool needsUpgrade)
        {
            needsUpgrade = false;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (string.IsNullOrEmpty(storedSalt))
            {
                needsUpgrade = IsLegacyMatch(password, storedHash);
                return needsUpgrade;
            }

            try
            {
                byte[] expectedHash = Convert.FromBase64String(storedHash);
                byte[] salt = Convert.FromBase64String(storedSalt);
                byte[] actualHash = DeriveKey(password, salt);
                return FixedTimeEquals(expectedHash, actualHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }

        private static bool IsLegacyMatch(string password, string storedHash)
        {
            if (FixedTimeEquals(password, storedHash))
            {
                return true;
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                string legacyIntHash = BitConverter.ToInt32(bytes, 0).ToString(CultureInfo.InvariantCulture);
                return FixedTimeEquals(legacyIntHash, storedHash);
            }
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(left), System.Text.Encoding.UTF8.GetBytes(right));
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            int diff = left.Length ^ right.Length;
            int length = Math.Min(left.Length, right.Length);

            for (int i = 0; i < length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }

    public readonly struct PasswordHash
    {
        public PasswordHash(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public string Hash { get; }
        public string Salt { get; }
    }
}
