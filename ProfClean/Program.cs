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
using Microsoft.Win32;

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
            string regPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";
            Console.WriteLine(Registry.GetValue(regPath, "", ""));

            foreach (string name in GetLocalUserNames())
            {
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find($"{name}");
                users.Remove(user);
                //working - need to remove dirs and maybe regkeys - hopefully reg keys are gone
                Console.WriteLine(name);
            }

            foreach (string sid in GetLocalUserSids())
            {
                
            }


            Console.Read();
        }

       internal static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {           
            Console.WriteLine("An error was written to the Error stream! " + e.ToString());
        }

        //build list of users on the computer
        private static List<string> GetLocalUserNames()
        {
            List<string> usernameList = new List<string>();
            List<string> sidList = new List<string>();
            SelectQuery query = new SelectQuery("Win32_UserProfile");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject sid in searcher.Get())
            {
                Console.WriteLine(sid);
                //string user = new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString();
                //int pos = user.LastIndexOf("\\") + 1;
                //string username = user.Substring(pos, user.Length - pos);                
                //if (username != "NEO" && username != "NETWORK SERVICE" && username != "LOCAL SERVICE" && username != "SYSTEM")
                //    //replace with ed_admin
                //{
                //    Console.WriteLine(username);
                //    NTAccount f = new NTAccount(username);
                //    SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                //    String sidString = s.ToString();
                //    sidList.Add(sidString);
                //    usernameList.Add(username);
                //}                
            }
            return usernameList;
        }

        //the error I was getting was caused because this application deleted the user from the machine but did not scrub the registry
        //this syntax:
        ////string user = new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString();
        //queries the registry to find users, so it could see the sid but not the user on the machine
        //this is fixed now

        //foreach user, need to delete registry key by sid and then delete user folder by username

        

        private static List<string> GetLocalUserSids()
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
                    Console.WriteLine(username);
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







