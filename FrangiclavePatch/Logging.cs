using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Frangiclave
{
    public static class Logging
    {
        private static StreamWriter _logWriter;
        
        private static void Log(string message, LogLevel level)
        {
            if (_logWriter == null)
            {
                _logWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, "frangiclave.log"));
            }
            var now = DateTime.Now.ToString(new CultureInfo("en-GB"));
            var levelLabel = level.ToString().ToUpper();
            _logWriter.WriteLine("[{0}] {1} - {2}", now, levelLabel, message);
            _logWriter.Flush();
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