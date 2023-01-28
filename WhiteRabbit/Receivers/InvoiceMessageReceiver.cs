using WhiteRabbit.Messaging.Abstractions;
using WhiteRabbit.Shared;

namespace WhiteRabbit.Receivers;

public class InvoiceMessageReceiver : IMessageReceiver<Invoice>
{
    private readonly ILogger logger;

    public InvoiceMessageReceiver(ILogger<InvoiceMessageReceiver> logger)
    {
        this.logger = logger;
    }

    public async Task ReceiveAsync(Invoice message, CancellationToken cancellationToken)
    {
        logger.LogInformation("INVOICE : Creating invoice for order {OrderNumber}...", message.OrderNumber);

        await Task.Delay(TimeSpan.FromMilliseconds(100 + Random.Shared.Next(10)));

        logger.LogInformation("INVOICE : End creating invoice for order {OrderNumber}", message.OrderNumber);
    }
}
