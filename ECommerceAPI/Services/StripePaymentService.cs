using Stripe;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services
{
    public class StripePaymentService
    {
        public PaymentIntent CreatePaymentIntent(decimal amount, string currency, string paymentMethodId)
        {
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe's amount unit is in cents
                Currency = currency,
                PaymentMethod = paymentMethodId,
                Confirm = true, // Automatically confirm the intent
                Description = "ECommerce Payment",
                ReturnUrl = "http://localhost:3000/" // Use your home page URL as the return URL
            };

            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = paymentIntentService.Create(paymentIntentOptions);

            return paymentIntent;
        }
    }

}
