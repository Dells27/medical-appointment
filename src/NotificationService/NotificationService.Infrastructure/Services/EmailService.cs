using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _configuration["Smtp:FromName"],
            _configuration["Smtp:FromEmail"]));

        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _configuration["Smtp:Host"],
            int.Parse(_configuration["Smtp:Port"]!),
            MailKit.Security.SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["Smtp:Username"],
            _configuration["Smtp:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}