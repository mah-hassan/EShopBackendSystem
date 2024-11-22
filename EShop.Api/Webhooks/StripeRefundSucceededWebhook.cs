using EShop.Domain.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace EShop.Api.Webhooks
{
    public sealed class StripeRefundSucceededWebhook
        : IEndpoint
    {
        public StripeRefundSucceededWebhook(IOptions<StripeSettings> options, ILogger<StripeRefundSucceededWebhook> logger)
        {
            stripeSettings = options.Value;
            this.logger = logger;
        }
        private readonly StripeSettings stripeSettings;
        private readonly ILogger<StripeRefundSucceededWebhook> logger;

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/webhook/stripe/refund-succeeded", async (HttpContext context) =>
            {
                using var streamReader = new StreamReader(context.Request.Body);

                var payload = await streamReader.ReadToEndAsync();

                var stripeEvent = EventUtility.ConstructEvent(payload,
                    context.Request.Headers["stripe-Signature"], stripeSettings.WebhookSecret,
                    throwOnApiVersionMismatch: false);

                if (stripeEvent != null && stripeEvent.Type is Events.RefundCreated)
                {
                    var refund = stripeEvent.Data.Object as Refund;
                    logger.LogInformation("Refund ID {refundId}, Amount: {amount}", refund?.Id, refund?.Amount);
                }
            });
        }
    }
}