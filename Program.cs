using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IRCTC_APP
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex to ensure single instance of the application
            using (Mutex mutex = new Mutex(true, "{Gadarpro1Mutex}", out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    // If the application is already running, show a message and exit
                    MessageBox.Show("The application is already running.");
                    return; // Exit the application
                }

                //Start monitoring in a separate thread
               // Thread monitorThread = new Thread(MonitorUnwantedTools);
                //monitorThread.IsBackground = true; // Background thread
                //monitorThread.Start();



                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }


        }
        static void MonitorUnwantedTools()
        {
            while (true)
            {
                try
                {
                            #if !DEBUG
                         // Check for debugger only in release mode
                         if (Debugger.IsAttached || Debugger.IsLogging())
                        {
                          ShowAlertAndExit("Debugger detected. Closing application.");
                        }
                          #endif

                    // Check for unwanted software
                    foreach (var process in Process.GetProcesses())
                    {
                        string processName = process.ProcessName.ToLower();
                        if (processName.Contains("fiddler") ||
                            processName.Contains("wireshark") ||
                            processName.Contains("httpdebugger") ||
                            processName.Contains("proxy") ||
                            processName.Contains("sniffer"))
                        {
                            ShowAlertAndExit($"Unwanted software detected: {processName}. Closing application.");
                        }
                    }
                }
                catch { /* Ignore any exceptions */ }

                Thread.Sleep(5000); // Check every 2 seconds
            }
        }

        static void ShowAlertAndExit(string message)
        {
           //MessageBox.Show(message, "Security Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Environment.Exit(0); // Close the application
        }
    }
}
