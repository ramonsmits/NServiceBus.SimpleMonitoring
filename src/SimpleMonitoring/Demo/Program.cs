using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        NServiceBus.Logging.LogManager.Use<NServiceBus.Logging.DefaultFactory>().Level(NServiceBus.Logging.LogLevel.Debug);
        var cfg = new EndpointConfiguration("Demo.SimpleMonitoring");
        cfg.UseContainer<StructureMapBuilder>();
        cfg.EnableInstallers();
        cfg.UsePersistence<InMemoryPersistence>();
        var t = cfg.UseTransport<MsmqTransport>().Transactions(TransportTransactionMode.SendsAtomicWithReceive);
        cfg.SendFailedMessagesTo("error");
        cfg.PurgeOnStartup(true);
        Console.WriteLine(" F - Fast message, processing duration 0.1s");
        Console.WriteLine(" S - Slow message, processing duration 3s");
        Console.WriteLine(" ESC - Exit");

        var instance = await Endpoint.Start(cfg).ConfigureAwait(false);

        try
        {
            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.F:
                        await instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(0.1) }).ConfigureAwait(false);
                        break;
                    default:
                        await instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(3) }).ConfigureAwait(false);
                        break;
                }
            }
        }
        finally
        {
            await instance.Stop().ConfigureAwait(false);
        }
    }
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
        await Task.Delay(message.Duration).ConfigureAwait(false);
        Console.WriteLine("Done!");
    }
}

