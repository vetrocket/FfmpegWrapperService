using System;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;

namespace FfmpegWrapperService
{
    /// <summary>
    /// Provides a Service wrapper for the class specified in _ServiceClass
    /// passes command line arguments after the first one to the service.
    /// </summary>
    class Program : ServiceBase
    {
        public const string SVCNAME = "FfmpegWrapper";
        public const string NICENAME = "Ffmpeg Wrapper";
        public const string SVCDESCRIPTION = "Encodes input files per configuration";
        private static readonly Type _ServiceClass = typeof(ServiceStarter);

        /// <summary>
        /// Save the commmand line arguments in memory for reference by onStart
        /// </summary>
        private static string[] _Args = null;
        private static Program _ServiceInstance = null;
        private static object _ServiceClassObject = null;

        internal static Program SvcInstance {
            get
            {
                return _ServiceInstance;
            }
        }

        /// <summary>
        /// This is only called when the program is loaded into memory as a service.
        /// </summary>
        public Program()
        {
            this.ServiceName = Program.SVCNAME;//this shows up in Event Log
            Program._ServiceInstance = this;
        }
        static int Main(string[] args)
        {
            _Args = args;
            if (System.Environment.UserInteractive || (args != null && args.Length > 0))
            {
                if (args != null && args.Length > 0)
                {
                    string parameter = "";
                    string[] paramArgs = null;
                    if (args.Length > 1)
                    {
                        paramArgs = new string[args.Length - 1];
                        Array.Copy(args, 1, paramArgs, 0, paramArgs.Length);
                        parameter = String.Join(" ", paramArgs);
                    }
                    switch (args[0])
                    {
                        case "--installSvc":
                            if (IsInstalled())
                            {
                                Console.WriteLine("Service already installed");
                                break;
                            }
                            if (!IsUserAdministrator())
                            {
                                Console.WriteLine("\r\n ** Must be run as Adminstrator to install ** ");
                                break;
                            }
                            Console.WriteLine("Starting installation...");
                            SvcInstaller.Install(parameter);
                            Console.WriteLine("Press any key to quit...");
                            Console.ReadKey();
                            //ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--uninstallSvc":
                            if (!IsUserAdministrator())
                            {
                                Console.WriteLine("\r\n ** Must be run as Adminstrator to un-install ** ");
                                return 0;
                            }
                            SvcInstaller.Uninstall();
                            //ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--console":
                            LoadAndRunServiceObject(_Args);
                            Console.WriteLine("Press any key to stop service...");
                            Console.ReadKey();
                            Program.StopService();
                            Console.WriteLine("Press any key to quit...");
                            Console.ReadKey();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("--installSvc | --uninstallSvc | --console ");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return 0;
                }
            }
            else
            {
                ServiceBase.Run(new Program());
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// This is triggered when the service is started or restarted
        /// </summary>
        /// <param name="args">these are only the temporary args from the service properties window</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                string[] currentArgs = _Args;
                if (args != null && args.Length > 0) currentArgs = args;
                base.OnStart(currentArgs);
                StartService(currentArgs);
            }
            catch (Exception x)
            {
                this.ExitCode = 1064;
                this.Stop();
                throw;
            }
        }
        protected override void OnStop()
        {
            StopService();
            base.OnStop();
        }

        private static void StartService(string[] args)
        {
            LoadAndRunServiceObject(args);
        }

        private static void LoadAndRunServiceObject(string[] args)
        {
            if (_ServiceClassObject == null)
            {
                try
                {
                    _ServiceClassObject = Activator.CreateInstance(_ServiceClass, new object[] { args });
                    string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string argStr = "";
                    if (args != null && args.Length > 0) argStr = " with arguments " + String.Join(",", args);
                }
                catch (Exception x)
                {
                    string msg = "Could not start service: " + x.Message;
                    if (x.InnerException != null)
                    {
                        msg += "\r\nInner Exception: " + x.InnerException.Message;
                    }
                    StopService();
                    throw;
                }
            }
            else
            {
                //log - already running!
            }
        }

        private static void StopService()
        {
            if (_ServiceClassObject != null)
            {
                if (_ServiceClassObject is IDisposable)
                {
                    IDisposable disposable = (IDisposable)_ServiceClassObject;
                    disposable.Dispose();
                }
                _ServiceClassObject = null;
            }
        }

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private static bool IsInstalled()
        {
            bool serviceExists = false;
            foreach (ServiceController sc in ServiceController.GetServices())
            {
                if (sc.ServiceName == SVCNAME)
                {
                    //service is found
                    serviceExists = true;
                    break;
                }
            }
            return serviceExists;
        }
    }
}
