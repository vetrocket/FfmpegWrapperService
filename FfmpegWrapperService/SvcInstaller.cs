using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Text;
using System.Reflection;

namespace FfmpegWrapperService
{
    [RunInstaller(true)]
    public class SvcInstaller : Installer
    {
        public SvcInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = Program.SVCNAME;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = Program.SVCDESCRIPTION;
            serviceInstaller.DisplayName = Program.NICENAME;
            this.AfterInstall += new InstallEventHandler(SvcInstaller_AfterInstall);

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = Program.SVCNAME;
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
        /// <summary>
        /// Start the service after installation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SvcInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(Program.SVCNAME))
            {
                try
                {
                    sc.Start();
                }
                catch (Exception x)
                {
                    Console.WriteLine("Could not start service: " + x.Message + "\r\n" + x.StackTrace);
                }
            }
        }

        public static void Install(string args)
        {
            TransactedInstaller ti = new TransactedInstaller();
            SvcInstaller mi = new SvcInstaller();
            ti.Installers.Add(mi);
            String path = String.Format("/assemblypath={0}", Assembly.GetExecutingAssembly().Location);
            String[] cmdline = { path };
            Console.WriteLine("Installing at path " + path + " with args " + args);
            InstallContext ctx = new InstallContext("", cmdline);
            //ctx.Parameters["assemblypath"] += "\" " + args;
            ti.Context = ctx;
            ti.Install(new System.Collections.Hashtable());
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            //using (ServiceController controller = new ServiceController(Program.SVCNAME))
            //{
            //    try
            //    {
            //        if (controller.Status == ServiceControllerStatus.Running | controller.Status == ServiceControllerStatus.Paused)
            //        {
            //            Program.WriteToLog("Service is running or paused; trying to stop service...");
            //            controller.Stop();
            //            controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 0, 30));
            //        }
            //        controller.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        Program.WriteToLog("Could not stop service for uninstallation: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            //    }
            //}
            base.Uninstall(savedState);
        }

        public static void Uninstall()
        {
            TransactedInstaller ti = new TransactedInstaller();
            SvcInstaller mi = new SvcInstaller();
            ti.Installers.Add(mi);
            String path = String.Format("/assemblypath={0}",
            System.Reflection.Assembly.GetExecutingAssembly().Location);
            String[] cmdline = { path };
            InstallContext ctx = new InstallContext("", cmdline);
            ti.Context = ctx;
            ti.Uninstall(null);
        }
    }
}
