# OrangeCabinet - C# .NET async udp server & client

[![nuget](https://badgen.net/nuget/v/OrangeCabinet/latest)](https://www.nuget.org/packages/OrangeCabinet/)
[![.NET CI](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml/badge.svg?branch=develop)](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml)
[![codecov](https://codecov.io/gh/shigenobu/OrangeCabinet/branch/develop/graph/badge.svg?token=RNH9EOC8JF)](https://codecov.io/gh/shigenobu/OrangeCabinet)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

## feature

OrangeCabinet is __'Asynchronous Programming Model (APM)'__ socket wrapper library,  
with __'Task-based Asynchronous Pattern (TAP)'__ at callback methods.  
Otherwise, __APM__ and __TAP__ mixed.

* Callback is below.
    * 'IncomingAsync' (received)
    * 'TimeoutAsync' (timeout)
    * 'ShutdownAsync' (shutdown)
* Can store user value in remote.
* Check timeout at regular intervals by last receive time.
* Client bind too, not connect. So, previously known client port.

## how to use

### callback

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

### for server (ip v4)

    public static async Task Main(string[] args)
    {
        var serverBinder = new OcBinder(new SampleCallback())
        {
            BindPort = 8710,
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        await server.SendToAsync("0", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8710));  // Send from server to some endpoint what you hope.
        server.WaitFor();
        // ...
        server.Shutdown();
    }

### for client (ip v4)

    public static async Task Main(string[] args)
    {
        using var clientBinder = new OcBinder(new Callback())
        {
            BindPort = 18710,
        };
        var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
        for (int j = 0; j < 3; j++)
        {
            await client.SendAsync($"{j}");
        }
    }

### for server (ip v6)

    public static async Task Main(string[] args)
    {
        var serverBinder = new OcBinder(new SampleCallback())
        {
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 8710,
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        await server.SendToAsync("0", new IPEndPoint(IPAddress.Parse("::1"), 8710));  // Send from server to some endpoint what you hope.
        server.WaitFor();
        // ...
        server.Shutdown();
    }

### for client (ip v6)

    public static async Task Main(string[] args)
    {
        using var clientBinder = new OcBinder(new Callback())
        {
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 18710,
        };
        var client = new OcRemote(clientBinder, "::1", 8710);
        for (int j = 0; j < 3; j++)
        {
            await client.SendAsync($"{j}");
        }
    }
