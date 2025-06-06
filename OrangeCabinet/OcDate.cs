namespace OrangeCabinet;

/// <summary>
///     Date.
/// </summary>
public static class OcDate
{
    /// <summary>
    ///     AddSeconds.
    ///     '0' is default, it's mean to 'utc'.
    /// </summary>
    public static double AddSeconds { get; set; }

    /// <summary>
    ///     Now.
    /// </summary>
    /// <returns>yyyy-MM-dd HH:mm:ss.fff</returns>
    internal static string Now()
    {
        var dateTimeOffset = DateTimeOffset.UtcNow;
        dateTimeOffset = dateTimeOffset.ToOffset(TimeSpan.FromSeconds(AddSeconds));
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
    }

    /// <summary>
    ///     Not timestamp milliseconds.
    /// </summary>
    /// <returns>milliseconds</returns>
    internal static long NowTimestampMilliSeconds()
    {
        var dateTimeOffset = DateTimeOffset.UtcNow;
        dateTimeOffset = dateTimeOffset.ToOffset(TimeSpan.FromSeconds(AddSeconds));
        return dateTimeOffset.ToUnixTimeMilliseconds();
    }
}