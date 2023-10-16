// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using MessageEntities;

var connectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNSTR");

ServiceBusProcessor processor;
var client = new ServiceBusClient(connectionString);
var queueName = "orders";
var subscriptionName = "testsubscriber";
// create a processor that we can use to process the messages
processor = client.CreateProcessor(queueName, subscriptionName);

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    var orderReceived = args.Message.Body.ToObjectFromJson<OrderDto>();
    Console.WriteLine($"Received order details Id: {orderReceived.Id} Name: {orderReceived.Name}");

    // complete the message. messages is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}