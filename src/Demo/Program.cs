using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;

Console.WriteLine(" F - Fast message, processing duration 0.1s");
Console.WriteLine(" S - Slow message, processing duration 3s");
Console.WriteLine(" ESC - Exit");

var host = Host.CreateDefaultBuilder(args)
    .UseNServiceBus(context =>
    {
        var cfg = new EndpointConfiguration("Demo.SimpleMonitoring");
        cfg.UseSerialization(new SystemJsonSerializer());
        cfg.EnableInstallers();
        cfg.EnableFeature<SimpleMonitoringFeature>();
        cfg.UseTransport(new LearningTransport());
        cfg.PurgeOnStartup(true);
        return cfg;
    })
    .Build();

await host.StartAsync();

var session = host.Services.GetService(typeof(IMessageSession)) as IMessageSession;

while (true)
{
    switch (Console.ReadKey().Key)
    {
        case ConsoleKey.Escape:
            await host.StopAsync();
            return;
        case ConsoleKey.F:
            await session!.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(0.1) });
            break;
        default:
            await session!.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(3) });
            break;
    }
}

sealed class TestMessage : IMessage
{
    public TimeSpan Duration { get; set; }
}

sealed class TestHandler : IHandleMessages<TestMessage>
{
    public async Task Handle(TestMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Processing for {message.Duration}");
        await Task.Delay(message.Duration, context.CancellationToken);
        Console.WriteLine("Done!");
    }
}
