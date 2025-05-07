namespace OrangeCabinet.Tests;

public class AsyncCallbackClient : OcCallback
{
    private const string Key = "inc";

    public override async Task IncomingAsync(OcRemote remote, byte[] message)
    {
        OcLogger.Info($"Received client: {message.OxToString()} ({remote})");

        var inc = remote.GetValue<int>(Key);
        inc++;
        remote.SetValue(Key, inc);

        await remote.SendAsync($"From client: {inc}".OxToBytes());
        if (inc > 10)
        {
            remote.ClearValue(Key);
            remote.Escape();
        }
    }

    public override Task TimeoutAsync(OcRemote remote)
    {
        OcLogger.Info($"By client, timeout: {remote}");
        return Task.CompletedTask;
    }

    public override Task ShutdownAsync(OcRemote remote)
    {
        OcLogger.Info($"By client, shutdown: {remote}");
        return Task.CompletedTask;
    }
}