using System.Security.Cryptography;
using System.Text;

namespace API_1.Security
{
    public class RSAEncryptOtp
    {
        public static string EncryptOtp(string password, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(password), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

    }

}
