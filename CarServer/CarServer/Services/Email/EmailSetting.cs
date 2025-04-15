namespace CarServer.Services.Email;
public class EmailSetting
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string DisplayName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string Password { get; set; } = null!;
}
