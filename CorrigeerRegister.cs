
/**
 *
 * willem Post
 * 16-2-2020
 * 
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderUSB
{
    public class CorrigeerRegister
    {
        public void CorrigeerReg(string[] mixList)
        {
            string strKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_08BB&PID_29C0&MI_00\";

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB\VID_08BB&PID_29C0&MI_00"))
                if (key != null)
                {

                    string friendlyKey = "";

                    var subkeys = key.GetSubKeyNames();
                    foreach (var subkey in subkeys)
                    {
                        int kanPort = (int)Registry.GetValue(strKey + subkey, "Address", null);


                        for (int t = 0; t < mixList.Length; t++)
                        {
                            if (kanPort == t + 1)
                            {
                                friendlyKey = mixList[t];
                            }
                        }
                        Registry.SetValue(strKey + subkey, "FriendlyName", friendlyKey);
                    }
                }
        }
    }
}

