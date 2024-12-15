namespace SecMan.Model
{
    public class Authorize
    {
        public string UserName { get; set; }=string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsNote { get; set; }
        public bool IsAuthorize { get; set; }
        public bool IsSigned { get; set; }

    }
    public class VerifySignature
    {
        public string Password { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class GetUserCredentialsDto
    {
        public ulong userId { get; set; }
        public string? Password { get; set; }
    }
}
