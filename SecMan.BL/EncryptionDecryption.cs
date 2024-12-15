using Microsoft.Extensions.Configuration;
using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Security.Cryptography;
using System.Text;

namespace SecMan.BL
{
    public enum EncryptionTypes
    {
        AESEncryption = 1,
        PBKDF2Hashing
    }

    public class EncryptionDecryption : IEncryptionDecryption
    {
        private readonly IConfiguration _configuration;
        private readonly int _saltSize;
        private readonly int _iterations;
        private readonly int _hashSize;

        public EncryptionDecryption(IConfiguration configuration)
        {
            _configuration = configuration;
            _saltSize = Convert.ToInt32(configuration["EncryptionConstants:SaltSize"]);
            _iterations = Convert.ToInt32(configuration["EncryptionConstants:Iterations"]);
            _hashSize = Convert.ToInt32(configuration["EncryptionConstants:HashSize"]);
        }


        public string EncryptPassword(string password, bool IsLegacy)
        {
            if (IsLegacy)
                return EncryptPasswordAES256(password, ((int)EncryptionTypes.AESEncryption).ToString());
            else
                return HashPasswordPBKDF2(password, ((int)EncryptionTypes.PBKDF2Hashing).ToString());
        }


        public bool VerifyPassword(string password, string oldPassword, bool IsLegacy)
        {
            try
            {
                if (IsLegacy)
                {
                    var decryptedPassword = DecryptPasswordAES256(oldPassword);
                    return decryptedPassword == password;
                }
                else
                    return VerifyHashPassword(oldPassword, password);
            }
            catch
            {
                return false;
            }
        }



        private string EncryptPasswordAES256(string password, string encryptionType)
        {
            using Aes aes = Aes.Create();
            string? keyString = _configuration["EncryptionConstants:SHAKey"];
            if (string.IsNullOrWhiteSpace(keyString))
            {
                throw new InvalidOperationException(EncryptionClassConstants.NullEncryptedString);
            }
            aes.Key = Encoding.UTF8.GetBytes(keyString);
            aes.GenerateIV();

            byte[] iv = aes.IV;
            byte[] encryptedPassword = Encrypt(password, aes.Key, iv);

            return $"${encryptionType}${ToHexString(aes.Key)}${ToHexString(iv)}${ToHexString(encryptedPassword)}";
        }


        public string? DecryptPasswordAES256(string encryptedString)
        {
            string[] parts = encryptedString.Split('$');
            if (parts.Length != 5)
                return null;

            byte[] key = FromHexString(parts[2]);
            byte[] iv = FromHexString(parts[3]);
            byte[] encryptedPassword = FromHexString(parts[4]);

            return Decrypt(encryptedPassword, key, iv);
        }


        private static byte[] Encrypt(string plaintext, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plaintext);
            }
            return ms.ToArray();
        }


        private static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(cipherText);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }


        private static string ToHexString(byte[] bytes)
        {
            StringBuilder sb = new(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }


        private static byte[] FromHexString(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException(EncryptionClassConstants.InvalidHexStringFormat);

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public string HashPasswordPBKDF2(string password, string id)
        {
            byte[] salt = new byte[_saltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = PBKDF2(password, salt, _iterations, _hashSize);

            string saltHex = BitConverter.ToString(salt).Replace("-", "").ToLower();
            string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return $"${id}${saltHex}${hashHex}";
        }


        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int hashSize)
        {
            using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(hashSize);
        }


        public bool VerifyHashPassword(string hashedPassword, string passwordToVerify)
        {
            string[] parts = hashedPassword.Split('$');
            if (parts.Length != 4)
            {
                return false;
            }

            string saltHex = parts[2];
            string storedHashHex = parts[3];

            byte[] salt = HexStringToByteArray(saltHex);
            byte[] storedHash = HexStringToByteArray(storedHashHex);

            byte[] hash = PBKDF2(passwordToVerify, salt, _iterations, _hashSize);

            return CryptographicEqual(storedHash, hash);
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            int numberOfChars = hex.Length;
            byte[] bytes = new byte[numberOfChars / 2];
            for (int i = 0; i < numberOfChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static bool CryptographicEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

    }
}
