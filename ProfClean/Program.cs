using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Management;
using System.Windows;
using System.Collections.Generic;

namespace ProfClean
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string name in GetLocalUserAccounts())
            {
                Console.WriteLine(name);
            }
            Console.Read();
        }

        //build list of users on the computer
        private static List<string> GetLocalUserAccounts()
        {
            List<string> userList = new List<string>();
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
                    userList.Add(username);
                }
            }
            return userList;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        public static extern bool DeleteProfile(string sidString, string profilePath, string omputerName);

    }
}
