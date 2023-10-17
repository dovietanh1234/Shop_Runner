using SHOP_RUNNER.Models.Email;

namespace SHOP_RUNNER.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel request);
        void sentOtp(string to, string body);
    }
}
