namespace ECommerceAPI.Models
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethodId { get; set; } // Changed from SourceToken to PaymentMethodId
    }
}
