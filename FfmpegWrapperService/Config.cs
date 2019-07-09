using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace FfmpegWrapperService

{
    internal static class Config
    {
        internal static int GetLoopSeconds()
        {
            int secs = 30;
            string secStr = ConfigurationManager.AppSettings["LoopSeconds"];
            int.TryParse(secStr, out secs);
            return secs;
        }
        internal static string GetCommandFilePath()
        {
            return ConfigurationManager.AppSettings["CommandFilePath"];
        }
        internal static string GetCommandString()
        {
            return ConfigurationManager.AppSettings["CommandString"];
        }
        internal static string GetInputDirectory()
        {
            return ConfigurationManager.AppSettings["InputDirectory"];
        }
        internal static string GetInputExtension()
        {
            return ConfigurationManager.AppSettings["InputExtension"];
        }
        internal static string GetOutFileExtension()
        {
            return ConfigurationManager.AppSettings["OutFileExtension"];
        }
    }
}
