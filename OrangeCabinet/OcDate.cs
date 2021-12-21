using System;

namespace OrangeCabinet
{
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
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            dateTimeOffset = dateTimeOffset.AddSeconds(AddSeconds);
            return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        ///     Not timestamp milli seconds.
        /// </summary>
        /// <returns>milli seconds</returns>
        internal static long NowTimestampMilliSeconds()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            dateTimeOffset = dateTimeOffset.AddSeconds(AddSeconds);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }
    }
}