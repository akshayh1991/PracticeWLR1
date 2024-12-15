using SecMan.Data.SQLCipher;
using SecMan.Interfaces.BL;
using System.Security.Cryptography;

namespace SecMan.BL
{
    public class RsaKeysBL : IRsaKeysBL
    {
        private readonly string? _publicKey;
        private readonly string? _privateKey;

        public RsaKeysBL()
        {
            Db _db = new Db();
            _publicKey = _db.RSAKeys.Select(x => x.PublicKey).FirstOrDefault();
            _privateKey = _db.RSAKeys.Select(x => x.PrivateKey).FirstOrDefault();
        }

        public RSA GetPrivateKey()
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(_privateKey);
            return rsa;
        }


        public RSA GetPublicKey()
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(_publicKey);
            return rsa;
        }

    }
}