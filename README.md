# OrangeCabinet - C# .NET async udp server & client

[![nuget](https://badgen.net/nuget/v/OrangeCabinet/latest)](https://www.nuget.org/packages/OrangeCabinet/)
[![.NET CI](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml/badge.svg?branch=develop)](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml)
[![codecov](https://codecov.io/gh/shigenobu/OrangeCabinet/branch/develop/graph/badge.svg?token=RNH9EOC8JF)](https://codecov.io/gh/shigenobu/OrangeCabinet)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

## feature

OrangeCabinet is __'Asynchronous Programming Model (APM)'__ socket wrapper library,  
with __'Task-based Asynchronous Pattern (TAP)'__ at callback methods.  
Otherwise, __APM__ and __TAP__ mixed.  
Sync methods (Incoming, Timeout and Shutdown) are disallowed for async override.   
If you want to use 'async',
Async methods (IncomingAsync, TimeoutAsync and ShutdownAsync) are override with 'UseAsyncCallback = true'.

* Callback is below.
    * 'Incoming or IncomingAsync' (received)
    * 'Timeout or TimeoutAsync' (timeout)
    * 'Shutdown or ShutdownAsync' (shutdown)
* Can store user value in remote.
* Check timeout at regular intervals by last receive time.
* Client bind too, not connect. So, previously known client port.

__(notice)__  

Synchronous methods are now obsolete.  
Please change to asynchronous methods.  

## how to use

### callback (sync)

    public class Callback : OcCallback
    {
        private const string Key = "inc";
        
        public override void Incoming(OcRemote remote, byte[] message)
        {
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(message)} ({remote})");
            
            int inc = remote.GetValue<int>(Key);
            inc++;
            remote.SetValue(Key, inc);
            
            remote.Send($"{inc}");
            if (inc > 10)
            {
                remote.ClearValue(Key);
                remote.Escape();
            }
        }

        public override void Timeout(OcRemote remote)
        {
            Console.WriteLine($"Timeout: {remote}");
        }

        public override void Shutdown(OcRemote remote)
        {
            Console.WriteLine($"Shutdown: {remote}");
        }
    }

### callback (async)

    public class AsyncCallback : OcCallback
    {
        private const string Key = "inc";

        public override bool UseAsyncCallback { get; init; } = true;

        public override async Task IncomingAsync(OcRemote remote, byte[] message)
        {
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(message)} ({remote})");

            int inc = remote.GetValue<int>(Key);
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

    public static void Main(string[] args)
    {
        var serverBinder = new OcBinder(new SampleCallback())
        {
            BindPort = 8710,
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        server.SendTo("0", new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8710));  // Send from server to some endpoint what you hope.
        server.WaitFor();
        // ...
        server.Shutdown();
    }

### for client (ip v4)

    public static void Main(string[] args)
    {
        using var clientBinder = new OcBinder(new Callback())
        {
            BindPort = 18710,
        };
        var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
        for (int j = 0; j < 3; j++)
        {
            client.Send($"{j}");
        }
    }

### for server (ip v6)

    public static void Main(string[] args)
    {
        var serverBinder = new OcBinder(new SampleCallback())
        {
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 8710,
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        server.SendTo("0", new IPEndPoint(IPAddress.Parse("::1"), 8710));  // Send from server to some endpoint what you hope.
        server.WaitFor();
        // ...
        server.Shutdown();
    }

### for client (ip v6)

    public static void Main(string[] args)
    {
        using var clientBinder = new OcBinder(new Callback())
        {
            SocketAddressFamily = OcSocketAddressFamily.Ipv6,
            BindPort = 18710,
        };
        var client = new OcRemote(clientBinder, "::1", 8710);
        for (int j = 0; j < 3; j++)
        {
            client.Send($"{j}");
        }
    }