//using orderUSB;
using OrderUSB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace USBorder
{
    public partial class Form1 : Form
    {
        static string path = Directory.GetCurrentDirectory();
        static string programFilePat = path;
        static string programFilePath = programFilePat + "\\";
        static int timerCount = 0;
        int corrected = 0;

        public Form1()
        {
            InitializeComponent();
        }


        // FORM1_LOAD --------------------------------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {

            timer1.Enabled = false;
            listBox1.HorizontalScrollbar = true;
            listBox1.RightToLeft = RightToLeft.No;

            DataSet dataSet = new DataSet();
            // Bestaat het bestand wel ?
            bool fileExists = (System.IO.File.Exists(programFilePath + "mixers.xml") ? true : false);

            if (fileExists)
            {
                dataSet.ReadXml(programFilePath + "mixers.xml");
                dataGridView1.DataSource = dataSet.Tables[0];
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            else
            {
                MessageBox.Show("File " + programFilePath + "mixers.xml" + " not found");
                Application.Exit();
            }


            //Detecteer vanuit het register welk type mixer er aangesloten is.
            MixerName mixer = new MixerName();
            //MixerName mixer = new MixerName();
            string mixertype = mixer.TypeOfMixer(programFilePath);
            
            if (mixertype != null)
            {
                dataGridView1.Columns[mixertype].DefaultCellStyle.BackColor = Color.LightGreen;
            }

            // Kollommen mogen niet gesorteerd kunnen worden !!
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // Rows nummering
            dataGridView1.RowHeadersWidth = 50;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }


        // START BUTTON ---------------------------------------------------------------------------------------------
        public void button1_Click(object sender, EventArgs e)
        {
            //Lees uit het datagridview welke namen er in de array gezet moeten worden. 
            MixerName mixer = new MixerName();
            string mx = mixer.TypeOfMixer(programFilePath);
            if (mx == null)
            {
                MessageBox.Show("error : mx = null");
                //Process.Start("USBConnection.Checker.one.exe");
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.FileName = "cmd";
                proc.WindowStyle = ProcessWindowStyle.Normal;
                proc.Arguments = "/C shutdown " + "-f -r -t 1";
                Process.Start(proc);
                Application.Exit();
            }

            //Traceer in welke kolom je moet zijn
            int typeMix = -1;
            for (int t = 0; t < dataGridView1.Columns.Count; t++)
            {
                if ((string)dataGridView1.Columns[t].Name == mx)
                {
                    typeMix++;
                }
            }

            //Lees de kolom met de cellen inhoud in de array 
            string[] GridArray = new string[dataGridView1.Rows.Count];
            for (int t = 0; t < dataGridView1.Rows.Count; t++)
            {
                GridArray[t] = (string)dataGridView1.Rows[t].Cells[typeMix].Value;
            }

            //Kloppen de namen uit de Array met wat er in het register staat (wrong ?)
            WrongKeyDetected wrong = new WrongKeyDetected();
            int wrongkey = wrong.WrongKey(GridArray);

            // lees de reboot stand uit readSettings.xml
            corrected = readXmlSettings("corrected");



            if (wrongkey == 1)
            {
                
                listBox1.Items.Add("Fail ! PCM2900 Registry keys are not the same as the XML table, Flag = " + (wrongkey));
                listBox1.Items.Add(mixer.TypeOfMixer(programFilePath) + " Detected ");
            }
            else
            {
                listBox1.Items.Add("Success ! PCM2900 Registry keys are the same as the XML table, Flag = " + (wrongkey));
                listBox1.Items.Add(mixer.TypeOfMixer(programFilePath) + " Detected ");
            }
            // Voorkeerde registersleutel gevonden !
            if (wrongkey > 0)
            {
                corrected++;
                //corrected2File(corrected);
                corrected2xmlFile(corrected);
            }

            // Geen voorkeerde registersleutel gevonden
            if (wrongkey == 0)
            {
                corrected = 0;
                //corrected2File(corrected);
                corrected2xmlFile(corrected);
            }

            // Houd bij hoeveel keer de pc opnieuw moet worden opgestart
            // clear de register sleutels en start de pc opnieuw op voor de 1e keer !
            if (corrected == 1)
            {
                corrected++;
                clearRegKeys();
                //corrected2File(corrected);
                corrected2xmlFile(corrected);
            }

            // Houd bij hoeveel keer de pc opnieuw moet worden opgestart
            // Zet de namen voor de kanalen goed en start de pc voor de tweede keer opnieuw op.
            else if (corrected > 1)
            {
                corrected = 0;
                //corrected2File(corrected);
                corrected2xmlFile(corrected);
                CorrigeerRegister corrReg = new CorrigeerRegister();
                //corrReg.CorrigeerReg(mixer.TypeOfMixer(), GridArray);
                corrReg.CorrigeerReg(GridArray);
                Reboot();
            }

        }

        // SAVE BUTTON -------------------------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();//create the data table
            dt.TableName = "USBnames";//give it a name

            //Maak het aantal kollommen aan
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dt.Columns.Add(dataGridView1.Columns[i].HeaderText);
            }

            //Loop door iedere cel van de DataGridView
            foreach (DataGridViewRow currentRow in dataGridView1.Rows)
            {
                dt.Rows.Add();
                int runningCount = 0;
                //loop bij iedere regel door elke kolom 
                foreach (DataGridViewCell item in currentRow.Cells)
                {
                    dt.Rows[dt.Rows.Count - 1][runningCount] = item.FormattedValue;
                    runningCount++;
                }
            }

            if (dt != null)
            {
                dt.WriteXml(programFilePath + "mixers.xml");
            }

            dataGridView1.RowHeadersWidth = 50;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            MessageBox.Show("Saved");
        }



        // RESET BUTTON zet de standaard namen terug---------------------------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {
            // Bestaat het bestand wel ?
            bool fileExists = (System.IO.File.Exists(programFilePath + "mixersbk.xml") ? true : false);

            if (fileExists)
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(programFilePath + "mixersbk.xml");
                dataGridView1.DataSource = dataSet.Tables[0];
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.RowHeadersWidth = 50;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                }
            }
            else
            {
                MessageBox.Show("File " + programFilePath + "mixersbk.xml" + " not found");
                Application.Exit();
            }
        }




        // }

        // TEST BUTTON -----------------------------------------------------------------------------------
        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }


        // CANCEL BUTTON ---------------------------------------------------------------------------------
        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            button5.Text = Convert.ToString("Cancel ");
            timerCount = 0;
        }

        // #########################################################################################################
        private void clearRegKeys()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.FileName = programFilePath + "ClearReset";
            process.Start();
        }

        private void Reboot()
        {
            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            //process.StartInfo.FileName = @"C:\USBDeview\Reboot";
            //process.Start();
            timer1.Enabled = true;
        }

        private void corrected2File(int w)
        {
            StreamWriter wrl = new StreamWriter(programFilePath + "corrected.txt");
            wrl.WriteLine(w);
            wrl.Close();
            //listBox1.Items.Add(" wrong flag = " + w);
        }

        private int readXmlSettings(string corrected)
        {

            // Bestaat het bestand wel ?
            bool fileExists = (System.IO.File.Exists(programFilePath + "Settings.xml") ? true : false);
            int corr = 0;
            if (fileExists)
            {
                //StreamReader rdc = new StreamReader(programFilePath + "corrected.txt");
                //corrected = Convert.ToInt32(rdc.ReadLine());
                //rdc.Close();

                XmlDocument XmlDoc = new XmlDocument(); // 
                XmlDoc.Load(programFilePath + "Settings.xml");
                XmlNodeList nodeList = XmlDoc.GetElementsByTagName("corrected");

                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i].InnerText.Length > 0)
                    {
                        corr = Convert.ToInt32(nodeList[i].InnerText);
                        //MessageBox.Show(nodeList[i].InnerText);
                    }
                }
            }
            else
            {
                MessageBox.Show("File " + programFilePath + "Settings.xml" + " not found");
                Application.Exit();
            }
            return corr;
        }

        private void corrected2xmlFile(int corrected)
        {
            DataTable dt3 = new DataTable();//create the data table
            dt3.TableName = "Settings";//give it a name
            dt3.Rows.Add();
            dt3.Columns.Add("comboBox1SelectedIndex");
            //dt3.Rows[0][0] = comboBox1.SelectedIndex;
            dt3.Columns.Add("corrected");
            dt3.Rows[0][1] = corrected;
            dt3.Columns.Add("willem");
            dt3.Rows[0][2] = "Post";

            if (dt3 != null)
            {
                dt3.WriteXml(programFilePath + "Settings.xml");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timerCount++;
            button5.Text = Convert.ToString("Reboot in 10 sec " + timerCount + " Cancel ");
            if (timerCount > 9)
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.FileName = "cmd";
                proc.WindowStyle = ProcessWindowStyle.Normal;
                proc.Arguments = "/C shutdown " + "-f -r -t 1";
                Process.Start(proc);
            }
        }

        //private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{

        //}

        //private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{

        //}

    }
}
