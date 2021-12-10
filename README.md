# OrangeCabinet - C# .NET async udp server & client

[![nuget](https://badgen.net/nuget/v/OrangeCabinet/latest)](https://www.nuget.org/packages/OrangeCabinet/)
[![.NET CI](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml/badge.svg?branch=develop)](https://github.com/shigenobu/OrangeCabinet/actions/workflows/ci.yaml)
[![codecov](https://codecov.io/gh/shigenobu/OrangeCabinet/branch/develop/graph/badge.svg?token=RNH9EOC8JF)](https://codecov.io/gh/shigenobu/OrangeCabinet)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

## feature

* Callback for 'Incoming'(received), 'Timeout'(timeout), 'Shutdown'(shutdown).
* Can store user value in remote.
* Check timeout at regular intervals by last receive time.
* Client bind too, not connect. So, previously known client port.

## how to use

### callback

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

### for server

    public static void Main(string[] args)
    {
        var serverBinder = new OcBinder(new SampleCallback())
        {
            BindPort = 8710,
        };
        var server = new OcLocal(serverBinder);
        server.Start();
        server.WaitFor();
        // ...
        server.Shutdown();
    }

### for client

    public static void Main(string[] args)
    {
        using var clientBinder = new OcBinder(new SampleCallback())
        {
            BindPort = 18710,
        };
        var client = new OcRemote(clientBinder, "127.0.0.1", 8710);
        for (int j = 0; j < 3; j++)
        {
            client.Send($"{j}".OxToBytes());
        }
    }
