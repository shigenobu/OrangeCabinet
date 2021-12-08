namespace OrangeCabinet
{
    public class OcStateReceive : OcState
    {
        public byte[]? Buffer { get; set; }

        public override string ToString()
        {
            return $"Socket: {Socket.OxSocketLocalEndPoint()}";
        }
    }
}