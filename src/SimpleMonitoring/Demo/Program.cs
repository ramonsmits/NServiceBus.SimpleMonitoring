using System;
using System.Threading;
using NServiceBus;

class Program
{
    static void Main()
    {
        NServiceBus.Logging.LogManager.Use<NServiceBus.Logging.DefaultFactory>().Level(NServiceBus.Logging.LogLevel.Debug);
        var cfg = new BusConfiguration();
        cfg.EnableInstallers();
        cfg.UsePersistence<InMemoryPersistence>();
        cfg.Transactions().DisableDistributedTransactions();
        cfg.PurgeOnStartup(true);
        Console.WriteLine(" F - Fast message, processing duration 0.1s");
        Console.WriteLine(" S - Slow message, processing duration 3s");
        Console.WriteLine(" ESC - Exit");
        using (var instance = Bus.Create(cfg).Start())
        {
            while (true)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.F:
                        instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(0.1) });
                        break;
                    default:
                        instance.SendLocal(new TestMessage { Duration = TimeSpan.FromSeconds(3) });
                        break;
                }
            }
        }
    }
}

class TestMessage : IMessage
{
    public TimeSpan Duration { get; set; }
}

class TestHandler : IHandleMessages<TestMessage>
{
    public void Handle(TestMessage message)
    {
        Console.WriteLine($"Processing for {message.Duration}");
        Thread.Sleep(message.Duration);
        Console.WriteLine("Done!");
    }
}

