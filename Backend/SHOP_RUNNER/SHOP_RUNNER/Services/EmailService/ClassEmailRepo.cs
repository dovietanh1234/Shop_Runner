using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using SHOP_RUNNER.Models.Email;
using MailKit.Net.Smtp;
using SHOP_RUNNER.DTOs.Order_DTO;
using SHOP_RUNNER.Entities;
using System.Text;

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

        public void sentSuccessOrder(string to, OrderDTO2 body, List<DtoDT> detail_o)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EMAIL:EmailUsername").Value)); // lay email cua minh trong file appSetting.json

            email.To.Add(MailboxAddress.Parse(to));

            email.Subject = "INVOICE SHOP RUNNER";

            // tạo một chuỗi để lặp giá trị:
            var ProductDetail1 = new StringBuilder();

            int totalAmount = 0;
            foreach ( var item in detail_o ) {
                int value = item.buy_qty * item.price;
                totalAmount += value;
                ProductDetail1.Append($"<tr><td>{item.ProductGetName.name}</td><td style=\"text-align:center\">{body.id}</td><td style=\"text-align:center\">{item.buy_qty}</td><td style=\"text-align:center\">${value}</td></tr>");
            }


            // DÙNG OrderDTO lấy các dữ liệu cần thiết đề gửi INVOICE đoạn này nhé
            double vat = (double)(totalAmount) * 0.10;
            double shipping = (double)(totalAmount) * 0.01;
            double t_m2 = (double)(totalAmount) + shipping + vat;

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<div style=\"width: 500px; margin: 10px auto; border: 3px solid #73AD21;\"> <div style=\"margin: auto 16px\"><div>" +
                $"<div style=\"display: flex; justify-content: space-between\"><h1 style=\"\">Shop Runner.</h1><h3 style=\"margin-top:30px\">" +
                $"Invoice</h3></div><div style=\"display: flex; justify-content: space-between\"><p style=\"\">" +
                $"shop runner<br/> 8 Ton That Thuyet, My Dinh,<br/> Nam Tu Niem Ha Noi Viet Nam <br/>HotLine: 09999888</p><p style=\"\">Order code: {body.invoiceId}<br/> Order date: {body.created_at}</p></div>" +
                $"</div><div><div style=\"display: flex; justify-content: space-between\"><div><h3 style=\"font-weight: bold\">BILLING INFROMATION</h3><p>Name: {body.user_entity.name},<br/>" +
                $" Phone: {body.tel},<br/> Company name: FPT APTECH,<br/> Street address: {body.shipping_address},<br/> Town/City: {body.Shipping.name},<br/> Country: Viet Nam,<br/> Postcode/ZIP: 10000</p></div>" +
                $"<div><h3 style=\"font-weight: bold\">PAYMENT</h3><p>Payment Method: {body.MethodPayment.name}, <br/>Order Status: Confirmed, <br/>Payment Status: {body.Status.name}, <br/>Shipping Method: Delivering</p>" +
                $"</div></div></div><div><table style=\"width:100%;\"><tr><th>Products</th><th>SKU</th><th>Quantity</th><th>SubTotal</th></tr>{ProductDetail1}</table></div><hr/><div><div style=\"display: flex; justify-content: space-between\"><div></div>" +
                $"<div style=\"line-height: 27px\"><span>Subtotal: ${totalAmount}</span> <br/><span>VAT 10%: ${vat}</span> <br/><span>Shipping: ${shipping}</span> <br/>" +
                $"<span>Total: ${t_m2}</span> <br/></div></div></div><div><p style=\"text-align: justify;\">Hello {body.user_entity.name}<br/>" +
                $"In my younger and more vulnerable years my father gave me some advice that I've been turning over in my mind ever since. " +
                $"'Whenever you feel like criticizing anyone,' he told me, 'just remember that all the people in this world haven't had the " +
                $"advantages that you've had</p></div><div><span>Best regards</span><br/><span>Email: conbonha2k@gmail.com<br/>Website: " +
                $"Shoprunner.com</span><br/><br/></div></div></div>"
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
