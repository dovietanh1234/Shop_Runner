using SHOP_RUNNER.DTOs.Order_DTO;
using SHOP_RUNNER.Models.Email;

namespace SHOP_RUNNER.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel request);
        void sentOtp(string to, string body);

        void sentSuccessOrder(string to, OrderDTO body);

        void sentCancelOrder(string to, string body);
    }
}
