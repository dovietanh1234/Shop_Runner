using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using SHOP_RUNNER.Models.Email;
using MailKit.Net.Smtp;
using SHOP_RUNNER.DTOs.Order_DTO;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.Services.EmailService
{
    public class ClassEmailRepo : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly RunningShopContext _context;

        public ClassEmailRepo(IConfiguration configuration, RunningShopContext context)
        {
            _configuration = configuration;
            _context = context;
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
                Text = $"<h1>Đoạn dữ liệu của bạn là:</h1></br><h3>{request.Body}</h3>"
            };

            // send this:
            // nhớ là sử dụng "SmtpClient" của framework: using MailKit.Net.Smtp;
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EMAIL:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EMAIL:EmailUsername").Value, _configuration.GetSection("EMAIL:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void sentOtp(string to, string body)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EMAIL:EmailUsername").Value)); // lay email cua minh trong file appSetting.json

            email.To.Add( MailboxAddress.Parse(to) );

            email.Subject = "OTP'S SHOP RUNNER";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<h1>Đoạn mã OTP của bạn là:</h1></br><h3>{body}</h3>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EMAIL:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EMAIL:EmailUsername").Value, _configuration.GetSection("EMAIL:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void sentSuccessOrder(string to, OrderDTO body)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EMAIL:EmailUsername").Value)); // lay email cua minh trong file appSetting.json

            email.To.Add(MailboxAddress.Parse(to));

            email.Subject = "INVOICE SHOP RUNNER";

            // DÙNG OrderDTO lấy các dữ liệu cần thiết đề gửi INVOICE đoạn này nhé

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<h1>YOUR ORDER IS DELIVERING:</h1></br><h3>address: {body.shipping_address}</h3><p>TEST THỬ</p>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EMAIL:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EMAIL:EmailUsername").Value, _configuration.GetSection("EMAIL:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void sentCancelOrder(string to, string body)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EMAIL:EmailUsername").Value)); // lay email cua minh trong file appSetting.json

            email.To.Add(MailboxAddress.Parse(to));

            email.Subject = "EMAIL FROM SHOP RUNNER";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>thank you to bought products from our store but some reasons your order is not success</p></br><p>the reason for this crash: </p><p>{body}</p><br/><p> sorry about this crash! <p>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EMAIL:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EMAIL:EmailUsername").Value, _configuration.GetSection("EMAIL:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }



    }
}
