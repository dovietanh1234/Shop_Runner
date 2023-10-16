using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using SHOP_RUNNER.Models.Email;
using MailKit.Net.Smtp;

namespace SHOP_RUNNER.Services.EmailService
{
    public class ClassEmailRepo : IEmailService
    {
        private readonly IConfiguration _configuration;

        public ClassEmailRepo(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //_configuration.GetSection("EMAIL:EmailHost").Value
        //_configuration.GetSection("EMAIL:EmailUsername").Value
        //_configuration.GetSection("EMAIL:EmailPassword").Value

        public void SendEmail(EmailModel request)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EMAIL:EmailUsername").Value));

            email.To.Add(MailboxAddress.Parse(request.to));

            email.Subject = request.subject;

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = request.Body
            };

            // send this:
            // nhớ là sử dụng "SmtpClient" của framework: using MailKit.Net.Smtp;
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EMAIL:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EMAIL:EmailUsername").Value, _configuration.GetSection("EMAIL:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
