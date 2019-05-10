using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;
using System.Management.Automation;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ProfClean
{


    //DEV NOTES:
        //encountering error after successfully deleting a local account via Powershell
        //this VS session has to run as admin for this program to execute successfully
            //probably need to reboot to resolve error   
        //this is close - need to figure out error and then perform more testing
            //confirmed remove-user works in PS ISE which should mean it will work here
        



    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string name in GetLocalUserAccounts())
            {
                //https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.powershell?view=pscore-6.2.0
                //https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.localaccounts/remove-localuser?view=powershell-5.1
                //https://blogs.msdn.microsoft.com/kebab/2014/04/28/executing-powershell-scripts-from-c/
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript($"Remove-LocalUser -sid {name}");
                    IAsyncResult result = PowerShellInstance.BeginInvoke();
                    PowerShellInstance.Streams.Error.DataAdded += Error_DataAdded;
                    while (result.IsCompleted == false)
                    {
                        Thread.Sleep(1000);
                        Console.WriteLine("Waiting for pipeline to finish...");
                    }
                    Console.WriteLine("Execution has stopped. The pipeline state is: " + PowerShellInstance.InvocationStateInfo.State);
                }
                Console.WriteLine(name);
            }
            Console.Read();
        }

       internal static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            // do something when an error is written to the error stream
            Console.WriteLine("An error was written to the Error stream!");
        }

        //build list of users on the computer
        private static List<string> GetLocalUserAccounts()
        {
            List<string> sidList = new List<string>();
            SelectQuery query = new SelectQuery("Win32_UserProfile");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject sid in searcher.Get())
            {
                string user = new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString();
                int pos = user.LastIndexOf("\\") + 1;
                string username = user.Substring(pos, user.Length - pos);                
                if (username != "NEO" && username != "NETWORK SERVICE" && username != "LOCAL SERVICE" && username != "SYSTEM")
                    //replace with ed_admin
                {                    
                    NTAccount f = new NTAccount(username);
                    SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                    String sidString = s.ToString();
                    sidList.Add(sidString);
                }                
            }
            return sidList;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        public static extern bool DeleteProfile(string sidString, string profilePath, string omputerName);

    }
}







