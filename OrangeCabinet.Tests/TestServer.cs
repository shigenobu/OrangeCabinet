using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestServer
    {
        public TestServer(ITestOutputHelper testOutputHelper)
        {
            OcDate.AddSeconds = 60 * 60 * 9;
            // OcLogger.Writer = new StreamWriter(new FileStream("OrangeCabinet.log", FileMode.Append));
            OcLogger.Verbose = true;
            OcLogger.StopLogger = true;
            OcLogger.StopLogger = false;
        }
        
        [Fact]
        public void TestForever()
        {
            var serverBinder = new OcBinder(new Callback())
            {
                BindPort = 8711
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            server.WaitFor();
        }

        // [Fact]
        // public void TestOnlyClient()
        // {
        //     using var clientBinder = new OcBinder(new Callback());
        //     var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
        //     for (int i = 0; i < 5; i++)
        //     {
        //         client.Send("hi\n".OxToBytes());
        //         Thread.Sleep(1000);
        //     }
        // }
    }

    public class Callback : OcCallback
    {
        public override void Incoming(OcRemote remote, byte[] message)
        {
            remote.ChangeIdleMilliSeconds(5000);
            OcLogger.Debug($"Incoming:{remote} {message.OxToString()}");
            remote.Send("hello\n".OxToBytes());
        }

        public override void Timeout(OcRemote remote)
        {
            OcLogger.Debug($"Timeout:{remote}");
        }

        public override void Shutdown(OcRemote remote)
        {
            OcLogger.Debug($"Shutdown:{remote}");
        }
    }
}