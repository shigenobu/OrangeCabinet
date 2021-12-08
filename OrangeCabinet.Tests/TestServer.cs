using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestServer
    {
        public TestServer(ITestOutputHelper testOutputHelper)
        {
            OcDate.AddSeconds = 60 * 60 * 9;
            OcLogger.Writer = new StreamWriter(new FileStream("OrangeCabinet.log", FileMode.Append));
            OcLogger.Verbose = true;
        }
        
        [Fact]
        public void TestForever()
        {
            var serverBinder = new OcBinder(new Callback())
            {
                BindPort = 8710
            };
            var server = new OcLocal(serverBinder);
            server.Start();

            // var clientBinder = new OcBinder(new Callback())
            // {
            //     BindPort = 18710
            // };
            // var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
            // client.Send("client".OxToBytes());
            
            server.WaitFor();
        }
    }

    public class Callback : OcCallback
    {
        public override void Incoming(OcRemote remote, byte[] message)
        {
            OcLogger.Debug($"Incoming:{remote} {message.OxToString()}");
            remote.Send("a".OxToBytes());
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