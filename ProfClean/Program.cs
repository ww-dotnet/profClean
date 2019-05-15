using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;
using System.Management.Automation;
using System.Collections.Generic;
using System.DirectoryServices;
using Microsoft.Win32;
using System.IO;
using System.DirectoryServices.ActiveDirectory;

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
            CleanUpAccounts();
            Console.WriteLine("test read");
            Console.Read();
        }

        //build list of users on the computer
        private static void CleanUpAccounts()
        {
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
                    RemoveUserRegistryEntry(sidString);
                    RemoveLocalUser(username);
                }
            }
        }



        private static void RemoveUserRegistryEntry(string sidString)
        {
            string keyName = $"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
            {
                Console.WriteLine("deleted registry key " + sidString);
                Console.WriteLine("test1");
                if (key != null)
                {
                    key.DeleteSubKey(sidString);
                }
            }
        }

        private static void RemoveLocalUser(string username)
        {
            string dir = $"C:\\Users\\{username}";
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find($"{username}");
            users.Remove(user); //remove the user account from the pc
            DelDir(dir);
        }

        private static void DelDir(string path)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C rd /S /Q {path}";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }



        internal static class NativeMethods
        {
            [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
            public static extern bool DeleteProfile(string sidString, string profilePath, string omputerName);

        }

    }
}







