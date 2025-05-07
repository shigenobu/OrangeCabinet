using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests;

public class TestV6Async
{
    public TestV6Async(ITestOutputHelper testOutputHelper)
    {
        OcDate.AddSeconds = 60 * 60 * 9;
        OcLogger.Writer = new StreamWriter(new FileStream("TestV6.log", FileMode.Append));
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
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 8710
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        // server.WaitFor();

        // -----
        using var clientBinder = new OcBinder(new AsyncCallbackClient())
        {
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 18710
        };
        var client = new OcRemote(clientBinder, "::1", 8710);
        for (var j = 0; j < 3; j++) await client.SendAsync($"{j}".OxToBytes());
        // -----

        // ...
        Thread.Sleep(1000);
        await server.SendToAsync("hello from server", new IPEndPoint(IPAddress.Parse("::1"), 8710));
        server.Shutdown();

        OcLogger.Close();
    }
}