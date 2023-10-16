namespace SHOP_RUNNER.Models.Users
{
    public class RefreshToken
    {
        public string refreshToken { get; set; } = string.Empty;

        public DateTime TokenCreated { get; set; } = DateTime.Now;    

        public DateTime TokenExpired { get; set; }


    }
}
