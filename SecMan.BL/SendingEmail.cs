using SecMan.Interfaces.BL;
using SecMan.Model;
using System.Net;
using System.Net.Mail;

namespace SecMan.BL
{
    public class SendingEmail : ISendingEmail
    {
        public async Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string resetLink, EmailConfiguration emailConfiguration, EmailMessage emailMessage)
        {
            string? fromEmail = emailConfiguration.From;
            string? netPassword = emailConfiguration.Password;
            string? smtpMailServer = emailConfiguration.MailServer;
            int smtpPort = emailConfiguration.SmtpPort;

            using (MailMessage mailMessage = new MailMessage())
            {
                if (fromEmail != null)
                {
                    mailMessage.From = new MailAddress(fromEmail);
                }
                mailMessage.To.Add(new MailAddress(recipientEmail));
                mailMessage.Subject = emailMessage.Subject;
                mailMessage.Body = $"{emailMessage.Body}<br><a href='{resetLink}'>{emailMessage.Title}</a>";
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.Normal;

                using (SmtpClient smtpClient = new SmtpClient(smtpMailServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(fromEmail, netPassword);
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(mailMessage);
                        return true;
                    }
                    catch (SmtpException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

    }
}
