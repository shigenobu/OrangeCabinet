using System.IO;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestSimple
    {
        public TestSimple(ITestOutputHelper testOutputHelper)
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
        public void Test()
        {
            var serverBinder = new OcBinder(new SampleCallback())
            {
                BindPort = 8710,
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            // server.WaitFor();
            
            // -----
            using var clientBinder = new OcBinder(new SampleCallback())
            {
                BindPort = 18710,
            };
            var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
            for (int j = 0; j < 3; j++)
            {
                client.Send($"{j}".OxToBytes());
            }
            // -----

            // ...
            Thread.Sleep(1000);
            server.Shutdown();
        }
    }
    
    public class SampleCallback : OcCallback
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