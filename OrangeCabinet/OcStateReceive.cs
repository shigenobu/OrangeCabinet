namespace OrangeCabinet;

/// <summary>
///     State receive.
/// </summary>
internal class OcStateReceive : OcState
{
    /// <summary>
    ///     Buffer.
    /// </summary>
    internal byte[]? Buffer { get; set; }

    /// <summary>
    ///     To string.
    /// </summary>
    /// <returns>socket local endpoint</returns>
    public override string ToString()
    {
        return $"Socket: {Socket.OxSocketLocalEndPoint()}";
    }
}