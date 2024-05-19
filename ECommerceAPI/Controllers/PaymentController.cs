using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Services;
using ECommerceAPI.Models;
using Microsoft.Extensions.Logging;
using System;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly StripePaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(StripePaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("charge")]
        public ActionResult Charge([FromBody] PaymentRequest request)
        {
            _logger.LogInformation($"Received payment intent request: {request.Amount}, {request.Currency}, {request.PaymentMethodId}");
            try
            {
                var paymentIntent = _paymentService.CreatePaymentIntent(request.Amount, request.Currency, request.PaymentMethodId);
                _logger.LogInformation($"Payment Intent created successfully: {paymentIntent.Id}");
                return Ok(new { paymentIntent.Id, paymentIntent.Status });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment intent: {ex}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
