using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Frangiclave
{
    public static class Logging
    {
        private static void Log(string message, LogLevel level)
        {
            var now = DateTime.Now.ToString(new CultureInfo("en-GB"));
            var levelLabel = level.ToString().ToUpper();
            using (var writer = File.AppendText(Path.Combine(Application.persistentDataPath, "frangiclave.log")))
            {
                writer.WriteLine($"[{now}] {levelLabel} - {message}");
            }
        }

        public static void Info(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        public static void Error(string message)
        {
            Log(message, LogLevel.Error);
        }
    }

    internal enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}
