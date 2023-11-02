using PayPalCheckoutSdk.Core;

namespace SHOP_RUNNER.Services.Order_service
{
    public class SaboxPaypal_class
    {
        private readonly IConfiguration _configuration;

        public SaboxPaypal_class(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PayPalHttpClient Client()
        {
            // _configuration.GetSection("PAYPAL:SecretKey").Value
            PayPalEnvironment _enviroment = new SandboxEnvironment(_configuration.GetSection("PAYPAL:PAYPAL-CLIENT-ID").Value, _configuration.GetSection("PAYPAL:PAYPAL-CLIENT-SECRET").Value);
            PayPalHttpClient client = new PayPalHttpClient(_enviroment);
            return client;
        }

    }
}
