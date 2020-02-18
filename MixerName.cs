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
using System.Xml;
//using System.Xml.XPath;
using System.Diagnostics;
//using System.Windows.Forms;
//using System.IO;

namespace OrderUSB
{
    public class MixerName
    {
        string mixerType;

        public string TypeOfMixer(string programFilePath)
        {
            //------------------------------------------------------------------------------------------------
            // Bepaal welke mixer er aan hangt Zoek naar de Atmel  (0805 1404 Webstation)  (0805 1402 Airlite)  (03EB 2402 Airence)  (0403 6001 Airlab) 
            //------------------------------------------------------------------------------------------------

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var keyusb = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))


                if (keyusb != null)
                {
                    var usbsubkeys = keyusb.GetSubKeyNames();
                    var usbKeys = keyusb.GetSubKeyNames();

                    XmlDocument XmlDoc = new XmlDocument(); // 
                    XmlDoc.Load(programFilePath + "mixers.xml");
                    XmlNodeList nodeList = XmlDoc.SelectNodes("/DocumentElement/USBnames");


                    string pidMixer = " ";
                    int f = 1;
                    foreach (XmlNode node in nodeList)
                    {
                        if (f == 1)
                        {
                            f = 0;
                            //Console.WriteLine(node.InnerText);
                            //Debug.Print(node.OuterXml);
                            pidMixer = node.OuterXml;
                        }
                    }

                    int t = 0;
                    int tt = 0;
                    string pidMixer1 = " ";
                    while ((pidMixer1.IndexOf("<USBn")) < 0)
                    {
                        t = pidMixer.IndexOf("/PID_", t);

                        if (t > 0) tt = pidMixer.IndexOf(">", t);
                        //string pidMixer2 = pidMixer.Substring(tt + 1, 8);

                        pidMixer1 = pidMixer.Substring(t + 1, tt - t - 1);
                        t++;


                        foreach (var usbsubkey in usbKeys)
                        {
                            string substr = pidMixer1.Substring(0, 8);
                            if (usbsubkey.Contains(substr))
                            {
                                mixerType = pidMixer1;
                                //mixerType = "PID_1402_Airlite";
                                //Debug.Print(pidMixer1);
                            }
                            //else if (usbsubkey == "VID_0805&PID_1404")
                            //{
                            //    mixerType = "Webstation";
                            //}
                        }

                    }

                }
            return mixerType;
        }
    }
}
