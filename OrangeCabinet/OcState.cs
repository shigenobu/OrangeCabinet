using System.Net.Sockets;

namespace OrangeCabinet;

/// <summary>
///     State.
/// </summary>
internal class OcState
{
    /// <summary>
    ///     Socket.
    /// </summary>
    internal Socket Socket { get; init; } = null!;
}