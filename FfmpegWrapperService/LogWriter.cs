using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace FfmpegWrapperService
{
    internal static class LogWriter
    {
        internal static object _lock = new object();
        internal enum LogLevel { Info=1, Warning=2, Error=3, Fatal=4 }

        internal static void WriteToLog(LogLevel level, string msg)
        {
            if (Program.SvcInstance != null)
            {
                WriteToEventLog(level, msg);
            }
            else
            {
                WriteToConsole(level, msg);
            }
        }

        private static void WriteToEventLog(LogLevel level, string msg)
        {
            lock (_lock)
            {
                System.Diagnostics.EventLogEntryType entryType = System.Diagnostics.EventLogEntryType.Error;
                switch (level)
                {

                    case LogLevel.Info:
                        entryType = System.Diagnostics.EventLogEntryType.Information;
                        break;
                    case LogLevel.Warning:
                        entryType = System.Diagnostics.EventLogEntryType.Warning;
                        break;
                    case LogLevel.Error:
                        entryType = System.Diagnostics.EventLogEntryType.Error;
                        break;
                    case LogLevel.Fatal:
                        entryType = System.Diagnostics.EventLogEntryType.Information;
                        break;
                    default:
                        break;
                }
                //should never be null by the time we get here
                if (Program.SvcInstance != null)
                {
                    Program.SvcInstance.EventLog.WriteEntry(msg, entryType);
                }
            }
        }
        private static void WriteToConsole(LogLevel level, string msg)
        {
            if (!Environment.UserInteractive)
            {
                throw new Exception("No User Interface present, cannot use console log");
            }

            lock (_lock)
            {
                var previous = Console.ForegroundColor;
                switch (level)
                {
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    default:
                        break;
                }
                Console.WriteLine(msg);
                Console.ForegroundColor = previous;
            }
        }
    }


}
