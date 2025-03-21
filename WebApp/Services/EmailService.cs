using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WebApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("WebApp", "dauthevu@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("dauthevu@gmail.com", "zgeo sgvq ojbv zsem");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log the error details
            Console.WriteLine($"Error sending email: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        var subject = "Xác nhận đăng ký tài khoản";
        var message = new StringBuilder();
        message.AppendLine("<h2>Xác nhận đăng ký tài khoản</h2>");
        message.AppendLine("<p>Vui lòng click vào link bên dưới để xác nhận đăng ký tài khoản của bạn:</p>");
        message.AppendLine($"<p><a href='{confirmationLink}'>{confirmationLink}</a></p>");
        message.AppendLine("<p>Link này sẽ hết hạn sau 24 giờ.</p>");
        message.AppendLine("<p>Nếu bạn không yêu cầu đăng ký tài khoản, vui lòng bỏ qua email này.</p>");

        await SendEmailAsync(email, subject, message.ToString());
    }
} 