namespace OrangeCabinet
{
    /// <summary>
    ///     State receive.
    /// </summary>
    public class OcStateReceive : OcState
    {
        /// <summary>
        ///     Buffer.
        /// </summary>
        public byte[]? Buffer { get; set; }

        /// <summary>
        ///     To string.
        /// </summary>
        /// <returns>socket local endpoint</returns>
        public override string ToString()
        {
            return $"Socket: {Socket.OxSocketLocalEndPoint()}";
        }
    }
}