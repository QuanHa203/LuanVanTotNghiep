using Microsoft.Extensions.Options;
using MimeKit;

namespace CarServer.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSetting _emailSetting;
    public EmailService(IOptions<EmailSetting> emailSettingOption)
    {
        _emailSetting = emailSettingOption.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_emailSetting.DisplayName, _emailSetting.SenderEmail));
        mimeMessage.To.Add(new MailboxAddress(toEmail, toEmail));
        mimeMessage.Subject = subject;

        BodyBuilder bodyBuilder = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = body
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync(_emailSetting.SmtpServer, _emailSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSetting.SenderEmail, _emailSetting.Password);
        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true);
    }
}
