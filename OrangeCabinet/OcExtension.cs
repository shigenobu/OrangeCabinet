using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OrangeCabinet;

/// <summary>
///     Extenstion.
/// </summary>
internal static class OcExtension
{
    /// <summary>
    ///     Byte[] to utf8 string.
    /// </summary>
    /// <param name="self">byte array</param>
    /// <returns>utf8 string</returns>
    /// <exception cref="OcExtensionException">error</exception>
    internal static string OxToString(this byte[] self)
    {
        try
        {
            return Encoding.UTF8.GetString(self);
        }
        catch (Exception e)
        {
            OcLogger.Error(e);
            throw new OcExtensionException(e);
        }
    }

    /// <summary>
    ///     Utf8 string to byte array.
    /// </summary>
    /// <param name="self">utf8 string</param>
    /// <returns>byte array</returns>
    /// <exception cref="OcExtensionException">error</exception>
    internal static byte[] OxToBytes(this string self)
    {
        try
        {
            return Encoding.UTF8.GetBytes(self);
        }
        catch (Exception e)
        {
            OcLogger.Error(e);
            throw new OcExtensionException(e);
        }
    }

    /// <summary>
    ///     Get socket locale endpoint.
    /// </summary>
    /// <param name="self">socket</param>
    /// <returns>locale endpoint or null</returns>
    internal static EndPoint? OxSocketLocalEndPoint(this Socket self)
    {
        return OcUtils.OrNull(() => self.LocalEndPoint);
    }

    /// <summary>
    ///     Get socket remote endpoint.
    /// </summary>
    /// <param name="self">socket</param>
    /// <returns>remote endpoint or null</returns>
    internal static EndPoint? OxSocketRemoteEndPoint(this Socket self)
    {
        return OcUtils.OrNull(() => self.RemoteEndPoint);
    }

    /// <summary>
    ///     Get host and port string.
    /// </summary>
    /// <param name="self">ip endpoint</param>
    /// <returns>host and port string</returns>
    internal static string OxToHostPort(this IPEndPoint self)
    {
        return $"{self.Address}:{self.Port}";
    }
}

/// <summary>
///     Extension exception.
/// </summary>
public class OcExtensionException : Exception
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="exception">error</param>
    internal OcExtensionException(Exception exception) : base(exception.ToString())
    {
    }
}