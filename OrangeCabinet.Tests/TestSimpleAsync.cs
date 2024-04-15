using System.IO;
using System.Net;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestSimpleAsync
    {
        public TestSimpleAsync(ITestOutputHelper testOutputHelper)
        {
            OcDate.AddSeconds = 60 * 60 * 9;
            // OcLogger.Writer = new StreamWriter(new FileStream("Test.log", FileMode.Append));
            OcLogger.Verbose = true;
            OcLogger.Transfer = new OcLoggerTransfer
            {
                Transfer = msg => testOutputHelper.WriteLine(msg.ToString()),
                Raw = false
            };
        }
        
        [Fact]
        public async Task Test()
        {
            var serverBinder = new OcBinder(new SampleAsyncCallback())
            {
                BindPort = 8710,
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            // server.WaitFor();
            
            // -----
            using var clientBinder = new OcBinder(new SampleAsyncCallback())
            {
                BindPort = 18710,
            };
            var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
            for (int j = 0; j < 3; j++)
            {
                await client.SendAsync($"{j}".OxToBytes());
            }
            // -----

            // ...
            Thread.Sleep(1000);
            server.SendTo("hello from server", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8710));
            server.Shutdown();
            
            OcLogger.Close();
        }
    }
    
    public class SampleAsyncCallback : OcCallback
    {
        private const string Key = "inc";

        public override bool UseAsyncCallback { get; init; } = true;

        public override async Task IncomingAsync(OcRemote remote, byte[] message)
        {
            OcLogger.Info($"Received: {message.OxToString()} ({remote})");
            
            int inc = remote.GetValue<int>(Key);
            inc++;
            remote.SetValue(Key, inc);
            
            await remote.SendAsync($"{inc}".OxToBytes());
            if (inc > 10)
            {
                remote.ClearValue(Key);
                remote.Escape();
            }
        }

        public override Task TimeoutAsync(OcRemote remote)
        {
            OcLogger.Info($"Timeout: {remote}");
            return Task.CompletedTask;
        }

        public override Task ShutdownAsync(OcRemote remote)
        {
            OcLogger.Info($"Shutdown: {remote}");
            return Task.CompletedTask;
        }
    }
}