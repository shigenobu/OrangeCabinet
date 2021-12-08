using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace OrangeCabinet
{
    /// <summary>
    ///     logger.
    /// </summary>
    public static class OcLogger
    {
        /// <summary>
        ///     log writer.
        ///     Default is stdout writer.
        ///     If set to null, no logging.
        /// </summary>
        public static TextWriter? Writer { get; set; } = new StreamWriter(Console.OpenStandardOutput());

        /// <summary>
        ///     Stop logger.
        ///     If true, all logging is stop except for Exception.
        /// </summary>
        public static bool StopLogger { get; set; }
        
        /// <summary>
        ///     Verbose.
        ///     If true, stop 'debug' logging.
        /// </summary>
        public static bool Verbose { get; set; }

        /// <summary>
        ///     Logging transfer action.
        ///     Mainly, for unit test.
        /// </summary>
        public static Action<object?>? Transfer;

        /// <summary>
        ///     Lock.
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        ///     Error.
        /// </summary>
        /// <param name="message">log message</param>
        public static void Error(object? message)
        {
            Out("ERROR", message);
        }
        
        /// <summary>
        ///     Info.
        /// </summary>
        /// <param name="message">log message</param>
        public static void Info(object? message)
        {
            Out("INFO", message);
        }
        
        /// <summary>
        ///     Debug.
        /// </summary>
        /// <param name="message">log message</param>
        public static void Debug(object? message)
        {
            if (!Verbose) return;
            Out("DEBUG", message);
        }

        /// <summary>
        ///     Debug.
        /// </summary>
        /// <param name="message">log func</param>
        public static void Debug(Func<object?> message)
        {
            if (!Verbose) return;
            Out("DEBUG", message.Invoke());
        }
        
        /// <summary>
        ///     Out to log.
        /// </summary>
        /// <param name="name">log name</param>
        /// <param name="message">log message</param>
        private static void Out(string name, object? message)
        {
            if (StopLogger && message is not Exception _) return;

            var context = default(OcLoggerContext);
            context.Recorded = OcDate.Now();
            context.ThreadId = $"{Thread.CurrentThread.ManagedThreadId:D10}";
            context.Name = name;
            if (message == null)
                context.Message = "<NULL>";
            else if (message.ToString()!.OxToBytes().Length == 0)
                context.Message = "<EMPTY>";
            else if (message is IEnumerable tmp)
                context.Message = string.Join("\n", tmp);
            else
                context.Message = message.ToString()!;
            
            StringBuilder builder = new();
            builder.Append($"[{context.Recorded}]");
            builder.Append($"[{context.ThreadId}]");
            builder.Append($"[{context.Name}]");
            builder.Append($"{context.Message}");
            var log = builder.ToString();
            Transfer?.Invoke(log);

            lock (Lock)
            {
                if (Writer != null)
                {
                    Writer.WriteLine(log);
                    Writer.Flush();    
                }    
            }
        }

        /// <summary>
        ///     Close logger.
        /// </summary>
        public static void Close()
        {
            lock (Lock)
            {
                if (Writer != null)
                {
                    Writer.Close();
                    Writer = null;
                }    
            }
        }
    }

    /// <summary>
    ///     Logger context.
    /// </summary>
    internal struct OcLoggerContext
    {
        /// <summary>
        ///     Recorded date time.
        /// </summary>
        internal string Recorded { get; set; }

        /// <summary>
        ///     Thread id.
        /// </summary>
        internal string ThreadId { get; set; }
        
        /// <summary>
        ///     Log name.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        ///     Log message.
        /// </summary>
        internal string Message { get; set; }
    }
}