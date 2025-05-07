using System.Net;
using System.Text;

namespace OrangeCabinet.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        OcDate.AddSeconds = 60 * 60 * 9;
        // OcLogger.Verbose = true;
        // OcLogger.StopLogger = true;

        var serverTask = StartServer();
        var clientTasks = StartClient();

        Task.WaitAll(clientTasks.ToArray());
        serverTask.Wait();

        OcLogger.Close();
    }

    private static Task StartServer()
    {
        return Task.Run(async () =>
        {
            var serverBinder = new OcBinder(new Callback())
            {
                BindPort = 8710
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            // server.WaitFor();
            await Task.Delay(1000);
            await server.SendToAsync("0", IPEndPoint.Parse("127.0.0.1:18170"));
        });
    }

    private static List<Task> StartClient()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 5; i++)
            tasks.Add(Task.Run(async () =>
            {
                using var clientBinder = new OcBinder(new Callback())
                {
                    BindPort = 18710
                };
                var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
                for (var j = 0; j < 3; j++) await client.SendAsync($"{j}");
                await Task.Delay(100);
            }));

        return tasks;
    }
}

public class Callback : OcCallback
{
    private const string Key = "inc";

    public override async Task IncomingAsync(OcRemote remote, byte[] message)
    {
        Console.WriteLine($"Received: {Encoding.UTF8.GetString(message)} ({remote})");

        var inc = remote.GetValue<int>(Key);
        inc++;
        remote.SetValue(Key, inc);

        await remote.SendAsync($"{inc}");
        if (inc > 10)
        {
            remote.ClearValue(Key);
            remote.Escape();
        }
    }

    public override Task TimeoutAsync(OcRemote remote)
    {
        Console.WriteLine($"Timeout: {remote}");
        return Task.CompletedTask;
    }

    public override Task ShutdownAsync(OcRemote remote)
    {
        Console.WriteLine($"Shutdown: {remote}");
        return Task.CompletedTask;
    }
}