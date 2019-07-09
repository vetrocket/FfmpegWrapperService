using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace FfmpegWrapperService
{
    public class ServiceStarter : IDisposable
    {
        private bool _IsDisposed = false;
        private volatile bool _Running = false;
        private Thread _RunThread = null;
        private const int _MAXWAIT = 60000;//one minute
        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            Stop();
            if (_IsDisposed)  return;
            if (disposing)
            {
                // Free any other managed objects here.
            }
            // Free any unmanaged objects here.
            _IsDisposed = true;
        }
        public ServiceStarter(string[] args)
        {
            _Running = true;
            _RunThread = new Thread(new ThreadStart(Run));
            _RunThread.Start();
        }

        internal void Stop()
        {
            _Running = false;
            int maxLoops = 30;//30 seconds to finish
            int numLoops = 0;
            while(_RunThread != null && _RunThread.IsAlive && numLoops < maxLoops)
            {
                System.Threading.Thread.Sleep(1000);
            }
            if (_RunThread.IsAlive)
            {
                try
                {
                    _RunThread.Abort();
                }
                catch { }
            }
            _RunThread = null;
        }
        internal void Run()
        {
            string commandFile = Config.GetCommandFilePath();
            string commandStr = Config.GetCommandString();
            string searchDir = Config.GetInputDirectory();
            string searchExt = Config.GetInputExtension();
            string outExt = Config.GetOutFileExtension();
            int sleepTime = Config.GetLoopSeconds() * 1000;
            if (!outExt.StartsWith(".")) outExt = "." + outExt;
            if (!searchExt.StartsWith(".")) searchExt = "." + searchExt;
            if(searchExt.ToLower() == outExt.ToLower())
            {
                throw new Exception("Search extentions must be different than output extension");
            }
            while (_Running)
            {
                foreach( var f in Directory.EnumerateFiles(searchDir, "*"+searchExt))
                {
                    try
                    {
                        ProcessFile(commandFile, commandStr, searchDir, outExt, f);
                    }
                    catch (Exception x)
                    {
                        LogWriter.WriteToLog(LogWriter.LogLevel.Error, "Error processing file: " + f + ":\r\n" + x.Message);
                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception x2) { }
                    }
                }
            }
            System.Threading.Thread.Sleep(sleepTime);
        }

        private static void ProcessFile(string commandFile, string commandStr, string searchDir, string outExt, string f)
        {
            string cmdArgs = commandStr.Replace("$INFILE", '"' + f + '"');
            string outFileName = Path.Combine(searchDir, Path.GetFileNameWithoutExtension(f) + outExt);
            cmdArgs = cmdArgs.Replace("$OUTFILE", '"' + outFileName + '"');
            Process fileProc = new Process();
            fileProc.StartInfo.FileName = '"' + commandFile + '"';
            fileProc.StartInfo.Arguments = cmdArgs;
            if (Environment.UserInteractive && Program.SvcInstance == null)
            {
                fileProc.StartInfo.UseShellExecute = true;
            }
            else
            {
                fileProc.StartInfo.UseShellExecute = false;
                fileProc.StartInfo.RedirectStandardOutput = true;
                fileProc.StartInfo.RedirectStandardError = true;
                fileProc.StartInfo.CreateNoWindow = true;
            }
            fileProc.Start();
            fileProc.WaitForExit(_MAXWAIT);
            if (!fileProc.HasExited)
            {
                LogWriter.WriteToLog(LogWriter.LogLevel.Warning, "Timed out processing file: " + f);
                fileProc.Kill();
                System.Threading.Thread.Sleep(10);
            }
            File.Delete(f);
        }
    }
}
