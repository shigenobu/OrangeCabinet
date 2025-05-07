using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests;

public class TestV4Async
{
    public TestV4Async(ITestOutputHelper testOutputHelper)
    {
        OcDate.AddSeconds = 60 * 60 * 9;
        OcLogger.Writer = new StreamWriter(new FileStream("TestV4.log", FileMode.Append));
        OcLogger.Verbose = true;
        // OcLogger.Transfer = new OcLoggerTransfer
        // {
        //     Transfer = msg => testOutputHelper.WriteLine(msg.ToString()),
        //     Raw = false
        // };
    }

    [Fact]
    public async Task Test()
    {
        var serverBinder = new OcBinder(new AsyncCallbackServer())
        {
            BindPort = 8710
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        // server.WaitFor();

        // -----
        using var clientBinder = new OcBinder(new AsyncCallbackClient())
        {
            BindPort = 18710
        };
        var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
        for (var j = 0; j < 3; j++) await client.SendAsync($"{j}".OxToBytes());
        // -----

        // ...
        Thread.Sleep(1000);
        await server.SendToAsync("hello from server", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8710));
        server.Shutdown();

        OcLogger.Close();
    }
}