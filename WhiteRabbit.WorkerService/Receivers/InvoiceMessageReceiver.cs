using WhiteRabbit.Messaging.Abstractions;
using WhiteRabbit.Shared;

namespace WhiteRabbit.WorkerService.Receivers;

public class InvoiceMessageReceiver : IMessageReceiver<Invoice>
{
    private readonly ILogger logger;

    public InvoiceMessageReceiver(ILogger<InvoiceMessageReceiver> logger)
    {
        this.logger = logger;
    }

    public async Task ReceiveAsync(Invoice message, CancellationToken cancellationToken)
    {
        if (message.OrderNumber % 5 == 0)
        {
            //Thread.Sleep(3000);
            throw new Exception("This is test");
        }

        logger.LogInformation("WS-INVOICE : Creating invoice for order {OrderNumber}...", message.OrderNumber);

        await Task.Delay(1000); //TimeSpan.FromSeconds(10 + Random.Shared.Next(10)));

        logger.LogInformation("WS-INVOICE : End creating invoice for order {OrderNumber}", message.OrderNumber);
    }
}
