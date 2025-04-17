using System.Security.Cryptography;

using System.Text;

namespace API_1.Security
{
    public class RSADecryptOtp
    {
        public static string DecryptOtp(string encryptedPassword, string privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedPassword), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
