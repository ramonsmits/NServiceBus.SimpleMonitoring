using System;
using System.Threading.Tasks;
using NServiceBus;

NServiceBus.Logging.LogManager.Use<NServiceBus.Logging.DefaultFactory>().Level(NServiceBus.Logging.LogLevel.Debug);
var cfg = new EndpointConfiguration("Demo.SimpleMonitoring");
cfg.UseSerialization(new SystemJsonSerializer());
cfg.EnableInstallers();
cfg.ReportLongRunningMessages(TimeSpan.FromSeconds(1));
var t = cfg.UseTransport<LearningTransport>();
cfg.PurgeOnStartup(true);
Console.WriteLine(" F - Fast message, processing duration 0.1s");
Console.WriteLine(" S - Slow message, processing duration 3s");
Console.WriteLine(" ESC - Exit");

var instance = await Endpoint.Start(cfg);

try
{
    while (true)
    {
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.Escape:
                return;
            case ConsoleKey.F:
                await instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(0.1) });
                break;
            default:
                await instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(3) });
                break;
        }
    }
}
finally
{
    await instance.Stop();
}

class TestMessage : IMessage
{
    public TimeSpan Duration { get; set; }
}

class TestHandler : IHandleMessages<TestMessage>
{
    public async Task Handle(TestMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Processing for {message.Duration}");
        await Task.Delay(message.Duration, context.CancellationToken);
        Console.WriteLine("Done!");
    }
}