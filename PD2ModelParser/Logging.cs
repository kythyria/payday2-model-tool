using System;
using System.Diagnostics;
using System.Reflection;

namespace PD2ModelParser
{
    public static class Log
    {
        private static ILogger _default;

        public static ILogger Default
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException("Default logger not set!");
                return _default;
            }

            set
            {
                if (_default != null)
                    throw new InvalidOperationException("Default logger already set!");
                _default = value ?? throw new InvalidOperationException("Cannot set default logger to null");
            }
        }
    }

    public enum LoggerLevel
    {
        Debug,
        Info,
        Status,
        Warn,
        Error
    }

    public interface ILogger
    {
        /// <summary>
        /// Prints or stores a log entry in whatever way is appropriate
        /// for the current logger.
        /// </summary>
        /// <param name="level">The severity of the message</param>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Log(LoggerLevel level, string message, params object[] value);

        /// <summary>
        /// Logs a very verbose and normally useless string, that's generally
        /// only useful for debugging.
        /// </summary>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Debug(string message, params object[] value);

        /// <summary>
        /// Logs a piece of information that is potentially useful for the user,
        /// but is generally not needed.
        /// </summary>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Info(string message, params object[] value);

        /// <summary>
        /// Logs the current status of the tool. This may be displayed on progress
        /// bars and the like, so it should not be called multiple times to display a
        /// single message.
        /// </summary>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Status(string message, params object[] value);

        /// <summary>
        /// Logs a warning. This is a very important message to warn the user an error
        /// has occured, but the program has recovered (however, invalid output may result).
        /// </summary>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Warn(string message, params object[] value);

        /// <summary>
        /// This operation has hit an unrecoverable error, and cannot continue.
        /// </summary>
        /// <param name="message">The format string to print</param>
        /// <param name="value">The parameters for the format string</param>
        void Error(string message, params object[] value);
    }

    public abstract class BaseLogger : ILogger
    {
        protected string GetCallerName(int level)
        {
            StackFrame frame = new StackFrame(level);
            MethodBase method = frame.GetMethod();
            return $"{method.DeclaringType?.Name}.{method.Name}";
        }

        public abstract void Log(LoggerLevel level, string message, params object[] value);

        public void Debug(string message, params object[] value)
        {
            Log(LoggerLevel.Debug, message, value);
        }

        public void Info(string message, params object[] value)
        {
            Log(LoggerLevel.Info, message, value);
        }

        public void Status(string message, params object[] value)
        {
            Log(LoggerLevel.Status, message, value);
        }

        public void Warn(string message, params object[] value)
        {
            Log(LoggerLevel.Warn, message, value);
        }

        public void Error(string message, params object[] value)
        {
            Log(LoggerLevel.Error, message, value);
        }
    }
}
