using SecMan.Model;

namespace SecMan.Interfaces.BL
{
    public interface ISendingEmail
    {
        Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string resetLink, EmailConfiguration emailConfiguration, EmailMessage emailMessage);
    }
}
