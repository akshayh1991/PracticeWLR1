namespace SecMan.Model
{
    public class EmailConfiguration
    {
        public string From { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MailServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
    }
}
