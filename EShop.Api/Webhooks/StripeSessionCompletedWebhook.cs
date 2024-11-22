using EShop.Application.Orders.Commands.CompleteCheckout;
using EShop.Domain.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace EShop.Api.Webhooks;

public sealed class StripeSessionCompletedWebhook
    : IEndpoint
{
    public StripeSessionCompletedWebhook(IOptions<StripeSettings> options, ILogger<StripeSessionCompletedWebhook> logger)
    {
        stripeSettings = options.Value;
        this.logger = logger;
    }
    private readonly StripeSettings stripeSettings;
    private readonly ILogger<StripeSessionCompletedWebhook> logger;
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("webhook/stripe/session-complete", async (HttpContext context, ISender sender) =>
        {
            using var streamReader =  new StreamReader(context.Request.Body);

            var payload = await streamReader.ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(payload,
                context.Request.Headers["stripe-Signature"], stripeSettings.WebhookSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent != null && stripeEvent.Type is Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                logger.LogInformation("Session ID {sessionId}, Status: {status}", session?.Id, session?.Status);
                var orderId = session?.Metadata["orderId"];
                if (!string.IsNullOrWhiteSpace(orderId))
                {
                    var command = new CompleteCheckoutCommand(Guid.Parse(orderId), session?.PaymentIntentId);
                    var result = await sender.Send(command);
                    if (result.IsFailure)
                    {
                        logger.LogError("Checkout completion faild, SessionId: {sessionId}, Error: {errorMessage}",
                            session?.Id, result.Errors?.First().Message);
                    }
                }
                return Results.Empty;
            }
            else 
            {
                logger.LogError("Invalid or unsupported Stripe event type: {eventType}", stripeEvent?.Type);
                return Results.Empty;
            }
        });
    }
}