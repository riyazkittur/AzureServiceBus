// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using MessageEntities;
using System.Text;
using System.Text.Json;

var connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNSTR");
var queueName = "orders";
// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the queue
ServiceBusSender sender;

// Create the clients that we'll use for sending and processing messages.
client = new ServiceBusClient(connectionString);
sender = client.CreateSender(queueName);

// create a batch 
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

for (int i = 1; i <= 4; i++)
{
    // try adding a message to the batch
    var orderItem = new OrderDto()
    {
        Id = i,
        Name = $"Order_{Guid.NewGuid()}",
        OrderItems = Enumerable.Range(0, i).Select(item => $"Item_{item}").ToList(),
        Region = i%3 == 0 ? "US": "IN"
    };
    var messageBody = JsonSerializer.Serialize(orderItem);
    var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
    {
        ContentType = "application/json",
        MessageId = i.ToString(),
       
    };

    // user-defined properties
    message.ApplicationProperties.Add("order-type", i%2 == 0 ? "New" : "Maintenance");
    if (!messageBatch.TryAddMessage(message))
    {
        // if an exception occurs
        throw new Exception($"Exception occurred while sending order {i}.");
    }
}

try
{
    // Use the producer client to send the batch of messages to the Service Bus queue
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of four messages has been published to the queue.");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await sender.DisposeAsync();
    await client.DisposeAsync();
}


Console.WriteLine("Press any key to continue");
Console.ReadKey();