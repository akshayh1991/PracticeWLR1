using System.Security.Cryptography;
using System.Text;

namespace SecMan.Model.Common
{
    public static class RsaKeyGenerator
    {
        public static void GenerateKeys()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                // Generate and export the private key in PEM format
                byte[] privateKey = rsa.ExportRSAPrivateKey();
                string privateKeyPem = ConvertToPem(privateKey, "RSA PRIVATE KEY");
                File.WriteAllText("private_key.pem", privateKeyPem);

                // Generate and export the public key in PEM format
                byte[] publicKey = rsa.ExportSubjectPublicKeyInfo();
                string publicKeyPem = ConvertToPem(publicKey, "PUBLIC KEY");
                File.WriteAllText("public_key.pem", publicKeyPem);

                Console.WriteLine("RSA key pair generated in PEM format.");
            }
        }


        private static string ConvertToPem(byte[] keyData, string keyType)
        {
            string base64Key = Convert.ToBase64String(keyData);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {keyType}-----");

            // Split the Base64 key into lines of 64 characters
            int lineLength = 64;
            for (int i = 0; i < base64Key.Length; i += lineLength)
            {
                sb.AppendLine(base64Key.Substring(i, Math.Min(lineLength, base64Key.Length - i)));
            }

            sb.AppendLine($"-----END {keyType}-----");
            return sb.ToString();
        }
    }
}
