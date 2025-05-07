namespace OrangeCabinet.Tests;

public class AsyncCallbackServer : OcCallback
{
    public override async Task IncomingAsync(OcRemote remote, byte[] message)
    {
        remote.ChangeIdleMilliSeconds(5000);
        OcLogger.Debug($"Incoming:{remote} {message.OxToString()}");
        await remote.SendAsync("hello\n".OxToBytes());
    }

    public override Task TimeoutAsync(OcRemote remote)
    {
        OcLogger.Debug($"Timeout:{remote}");
        return Task.CompletedTask;
    }

    public override Task ShutdownAsync(OcRemote remote)
    {
        OcLogger.Debug($"Shutdown:{remote}");
        return Task.CompletedTask;
    }
}