using System.Security.Cryptography;

namespace SecMan.Interfaces.BL
{
    public interface IRsaKeysBL
    {
        RSA GetPrivateKey();
        RSA GetPublicKey();
    }
}
