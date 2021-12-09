using System.Net.Sockets;

namespace OrangeCabinet
{
    /// <summary>
    ///     State.
    /// </summary>
    public class OcState
    {
        /// <summary>
        ///     Socket.
        /// </summary>
        public Socket Socket { get; init; } = null!;
    }
}