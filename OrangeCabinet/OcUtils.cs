using System;
using System.Text;

namespace OrangeCabinet;

/// <summary>
///     Utils.
/// </summary>
internal static class OcUtils
{
    /// <summary>
    ///     Random chars.
    /// </summary>
    private const string RandomChars = "0123456789abcedfghijklmnopqrstuvwxyzABCEDFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    ///     Make random string.
    /// </summary>
    /// <param name="length">length</param>
    /// <returns>random string</returns>
    internal static string RandomString(int length)
    {
        if (length < 1)
            return string.Empty;

        var randomCharLen = RandomChars.Length;
        var builder = new StringBuilder(length);
        var random = new Random();

        for (var i = 0; i < length; i++)
        {
            var idx = random.Next(randomCharLen);
            builder.Append(RandomChars[idx]);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Make random int.
    /// </summary>
    /// <param name="min">min</param>
    /// <param name="max">max</param>
    /// <returns>random int</returns>
    internal static int RandomInt(int min, int max)
    {
        return new Random().Next(min, max);
    }

    /// <summary>
    ///     Or null.
    /// </summary>
    /// <param name="func">func</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>invoke result or null</returns>
    internal static T? OrNull<T>(Func<T> func)
    {
        try
        {
            return func.Invoke();
        }
        catch (Exception e)
        {
            OcLogger.Debug(() => e);
        }

        return default;
    }
}