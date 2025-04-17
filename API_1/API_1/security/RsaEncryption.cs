namespace API_1.Security
{
    public class RsaEncryption
    {
        private static readonly IConfiguration Configuration;

        static RsaEncryption()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static string GetPublicKey()
        {
            string publickey= Configuration["RSA:PublicKey"];
            if (string.IsNullOrEmpty(publickey))
            {
                return "public key not found";
            }
            return publickey;
        }

        public static string GetPrivateKey()
        {
            string privatekey = Environment.GetEnvironmentVariable("RSA_PRIVATE_KEY");
            if (string.IsNullOrEmpty(privatekey))
            {
                return "private key not found";
            }
            return privatekey;
        }
    }
}
