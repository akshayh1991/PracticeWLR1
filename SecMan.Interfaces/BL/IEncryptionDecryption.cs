namespace SecMan.Interfaces.BL
{
    public interface IEncryptionDecryption
    {
        string EncryptPassword(string password, bool IsLegacy);
        string? DecryptPasswordAES256(string encryptedString);
        bool VerifyHashPassword(string hashedPassword, string passwordToVerify);
        bool VerifyPassword(string password, string oldPassword, bool IsLegacy);
    }
}
