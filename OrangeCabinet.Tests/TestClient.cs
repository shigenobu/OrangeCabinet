using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OrangeCabinet.Tests
{
    public class TestClient
    {
        public TestClient(ITestOutputHelper testOutputHelper)
        {
            OcDate.AddSeconds = 60 * 60 * 9;
            OcLogger.Writer = new StreamWriter(new FileStream("OrangeCabinet.log", FileMode.Append));
            // OcLogger.Verbose = true;
        }

        [Fact]
        public void TestServerClient()
        {
            var serverBinder = new OcBinder(new ServerCallback())
            {
                BindHost = "0.0.0.0",
                BindPort = 8710,
                ReadBufferSize = 128,
                Divide = 5
            };
            var server = new OcLocal(serverBinder);
            server.Start();
            Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    OcLogger.Info($"Count: {serverBinder.GetRemoteCount()}");
                    Thread.Sleep(1000);
                }
                server.Shutdown();
            });
            
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                var task = new Task((idx) =>
                {
                    int tmp = (int)idx;
                    using var clientBinder = new OcBinder(new ClientCallback())
                    {
                        BindHost = "0.0.0.0",
                        BindPort = 18710 + tmp,
                        ReadBufferSize = 128,
                        Divide = 1
                    };
                    var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
                    OcLogger.Info($"Local:{client.LocalEndpoint} Remote:{client.RemoteEndpoint}");
                    client.ChangeIdleMilliSeconds(5000);
                    for (int j = 0; j < 3; j++)
                    {
                        client.Send($"c:{tmp}-{j}".OxToBytes());
                        // Thread.Sleep(1000);
                    }
                    Thread.Sleep(2000);
                }, i);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks.ToArray());
            server.WaitFor();
        }
    }

    public class ServerCallback : OcCallback
    {
        private const string Key = "inc";
        
        public override void Incoming(OcRemote remote, byte[] message)
        {
            OcLogger.Info($"Received server: {message.OxToString()} ({remote})");
            remote.ChangeIdleMilliSeconds(3000);
            
            int inc = remote.GetValue<int>(Key);
            inc++;
            remote.SetValue(Key, inc);
            
            remote.Send($"From server: {inc}".OxToBytes());
            if (inc > 10) remote.Escape();
        }

        public override void Timeout(OcRemote remote)
        {
            OcLogger.Info($"By server, timeout: {remote}");
        }

        public override void Shutdown(OcRemote remote)
        {
            OcLogger.Info($"By server, shutdown: {remote}");
        }
    }
    
    public class ClientCallback : OcCallback
    {
        private const string Key = "inc";
        
        public override void Incoming(OcRemote remote, byte[] message)
        {
            OcLogger.Info($"Received client: {message.OxToString()} ({remote})");
            
            int inc = remote.GetValue<int>(Key);
            inc++;
            remote.SetValue(Key, inc);
            
            remote.Send($"From client: {inc}".OxToBytes());
            if (inc > 10)
            {
                remote.ClearValue(Key);
                remote.Escape();
            }
        }

        public override void Timeout(OcRemote remote)
        {
            OcLogger.Info($"By client, timeout: {remote}");
        }

        public override void Shutdown(OcRemote remote)
        {
            OcLogger.Info($"By client, shutdown: {remote}");
        }
    }
}