using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.DTOs.Order_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Email;

namespace SHOP_RUNNER.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel request);
        void sentOtp(string to, string body);

        void sentSuccessOrder(string to, OrderDTO2 body, List<DtoDT> detail_o);

        void sentCancelOrder(string to, string body);
    }
}
