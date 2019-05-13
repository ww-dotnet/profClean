using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;
using System.Management.Automation;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.DirectoryServices;

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
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find($"{name}");
                users.Remove(user);
                //working - need to remove dirs and maybe regkeys - hopefully reg keys are gone
                Console.WriteLine(name);
            }
            Console.Read();
        }

       internal static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {           
            Console.WriteLine("An error was written to the Error stream! " + e.ToString());
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
                    //Console.WriteLine(username);
                    //NTAccount f = new NTAccount(username);
                    //SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                    //String sidString = s.ToString();
                    sidList.Add(username);
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







