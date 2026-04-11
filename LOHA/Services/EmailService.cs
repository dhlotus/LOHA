using System.Net;
using System.Net.Mail;
using LOHA.Models;
using Microsoft.Extensions.Options;

namespace LOHA.Services
{
    /// <summary>
    /// Service xử lý gửi email
    /// </summary>
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        // Constructor: nhận cấu hình email từ appsettings.json
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        /// <summary>
        /// Gửi email
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="body">Nội dung email (hỗ trợ HTML)</param>
        /// <returns>true nếu gửi thành công, false nếu thất bại</returns>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Tạo đối tượng SmtpClient
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    // Cấu hình xác thực
                    client.EnableSsl = true; // Gmail yêu cầu SSL
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        _emailSettings.SenderEmail,
                        _emailSettings.SenderPassword
                    );

                    // Tạo nội dung email
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true // Cho phép dùng HTML trong nội dung
                    };
                    mailMessage.To.Add(toEmail);

                    // Gửi email (bất đồng bộ)
                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra console
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                return false;
            }
        }
    }
}