using System.IO;
using System.Net;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestSimpleV6
    {
        public TestSimpleV6(ITestOutputHelper testOutputHelper)
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
        public void Test()
        {
            var serverBinder = new OcBinder(new SampleV6Callback())
            {
                SocketAddressFamily = OcSocketAddressFamily.Ipv6,
                BindPort = 8710,
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            // server.WaitFor();
            
            // -----
            using var clientBinder = new OcBinder(new SampleV6Callback())
            {
                SocketAddressFamily = OcSocketAddressFamily.Ipv6,
                BindPort = 18710,
            };
            var client = new OcRemote(clientBinder, "::1", 8710);
            for (int j = 0; j < 3; j++)
            {
                client.Send($"{j}".OxToBytes());
            }
            // -----

            // ...
            Thread.Sleep(1000);
            server.SendTo("hello from server", new IPEndPoint(IPAddress.Parse("::1"), 8710));
            server.Shutdown();
            
            OcLogger.Close();
        }
    }
    
    public class SampleV6Callback : OcCallback
    {
        private const string Key = "inc";
        
        public override void Incoming(OcRemote remote, byte[] message)
        {
            OcLogger.Info($"Received: {message.OxToString()} ({remote})");
            
            int inc = remote.GetValue<int>(Key);
            inc++;
            remote.SetValue(Key, inc);
            
            remote.Send($"{inc}".OxToBytes());
            if (inc > 10)
            {
                remote.ClearValue(Key);
                remote.Escape();
            }
        }

        public override void Timeout(OcRemote remote)
        {
            OcLogger.Info($"Timeout: {remote}");
        }

        public override void Shutdown(OcRemote remote)
        {
            OcLogger.Info($"Shutdown: {remote}");
        }
    }
}