using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderUSB
{
    public class WrongKeyDetected
    {
        public int WrongKey(string[] mixList)
        {
            //------------------------------------------------------------------------------------------------
            // Lees de friendly keys uit het register en vergelijk ze met de vorige keer
            //------------------------------------------------------------------------------------------------

            int wrong = 0;

            string strKeyLst = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_08BB&PID_29C0&MI_00\";

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB\VID_08BB&PID_29C0&MI_00"))
                if (key != null)
                {
                    string result;

                    var subkeys = key.GetSubKeyNames();
                    foreach (var subkey in subkeys)
                    {
                        int kanPort = (int)Registry.GetValue(strKeyLst + subkey, "Address", null);

                        for (int t = 0; t < mixList.Length; t++)
                        {
                            if (kanPort == t + 1)
                            {
                                result = (string)Registry.GetValue(strKeyLst + subkey, "FriendlyName", null);
                                if (result != mixList[t]) wrong++;
                            }
                        }
                    }
                }
            return wrong;
        }
    }
}
