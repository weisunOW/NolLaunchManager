using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace NolMultiLauncherAllServer
{
    public partial class Form1 : Form
    {
        private List<string> filePaths;
        private List<int> openedProcessId;
        Form2 form2;
        public Form1()
        {
            InitializeComponent();
            InitForm();
        }
        ~Form1() 
        {

            Dispose(false);
        }
        /*
         * Module Name: InitForm
         * Author: Mama Zhang
         * Purpose: Initialize the form to load registry entries of all server information.
         * Caller: Form1()
         * Note:
         * Fantasy registry entry point: TecmoKoei 
         * Pheonix registry entry point: KOEI
         * Registry tree structures between thess two are similar, Fantasy version uses TecmoKoei as an entry point and has more settings on graphics effect.
         * 吐槽1：为什么我装完高清版以后，注册表里面没有GameFolder啊啊啊啊啊！！！ 
         * 吐槽2：重装了梦幻高清客户端才搞定了以上的问题，难道第一次装的不对？
         * 
         */
        private void InitForm() 
        {
            this.toolStripStatusLabel1.Text = "初始化中...";
            this.radioButton5.Checked = true;
            this.radioButton6.Checked = false;
            this.checkBox1.Checked = true;
            this.filePaths = new List<string>();
            this.openedProcessId = new List<int>();
            this.toolStripStatusLabel1.Text = "查找信长目录中...";
            findNolInstallationFolder();
            this.toolStripStatusLabel1.Text = "信长目录添加完毕。";
            string pathToSave = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fileName = Path.Combine(pathToSave, Program.SETTINGS_FILENAME);
            if(File.Exists(fileName))
                loadSettings(fileName);
        }
        private void findNolInstallationFolder() 
        {
            try 
            {
                getGameFolders();
                foreach (string fp in filePaths)
                {
                    this.comboBox1.Items.Add(fp);
                }
                this.comboBox1.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to get Nol installation folder from the registry. Fault information: \n" + ex.ToString());
            }
        }
        private void getGameFolders() 
        {
            RegistryKey nolClient = null; 
            string nolSDKEY = Program.NOL_REG_SOFTWARE+"\\"+Program.NOL_REG_OLD_ENTRYPOINT;
            string nolHDKEY = Program.NOL_REG_SOFTWARE+"\\"+Program.NOL_REG_NEW_ENTRYPOINT;
            string gameFolder = "";
            string filePath = "";
            // Find file path for Nobunaga Online SD clients for all servers.
            for (int i = 0; i < Program.NOL_REG_CLIENTS_SD.Length; i++)
            {
                toolStripStatusLabel1.Text = "检查 " + Program.NOL_REG_CLIENTS_SD[i] + "。";
                try
                {
                    nolClient = Registry.CurrentUser.OpenSubKey(nolSDKEY + "\\" + Program.NOL_REG_CLIENTS_SD[i], false);
                }
                catch (NullReferenceException nre)
                {
                    MessageBox.Show("Oops, 我搞不定了，你自己指定吧！嘤嘤嘤\n 问题原因是：" + nre.StackTrace);
                }
                if (nolClient != null)
                {
                    toolStripStatusLabel1.Text = "添加 " + Program.NOL_REG_CLIENTS_SD[i] + "。";
                    gameFolder = (string)nolClient.GetValue(Program.NOL_REG_GAMEFOLDER_NAME, "Not Existed");
                    if(gameFolder != "Not Existed")
                    {
                        filePath = gameFolder + "\\" + Program.NOL_SD_PROCESS_NAME;
                        if(File.Exists(filePath))
                            this.filePaths.Add(filePath);
                    }   
                }
            }
            // Find file path for Nobunaga Online HD clients for all servers.
            for (int i = 0; i < Program.NOL_REG_CLIENTS_HD.Length; i++)
            {
                toolStripStatusLabel1.Text = "检查 "+Program.NOL_REG_CLIENTS_HD[i]+"。";
                try
                {
                    nolClient = Registry.CurrentUser.OpenSubKey(nolHDKEY + "\\" + Program.NOL_REG_CLIENTS_HD[i], true);
                }
                catch (NullReferenceException nre)
                {
                    MessageBox.Show("Oops, 我搞不定了，你自己指定信长目录吧！嘤嘤嘤\n 问题原因是：" + nre.StackTrace);
                }
                if (nolClient != null)
                {
                    toolStripStatusLabel1.Text = "添加 " + Program.NOL_REG_CLIENTS_HD[i] + "。";
                    gameFolder = (string)nolClient.GetValue(Program.NOL_REG_GAMEFOLDER_NAME,"Not Existed");
                    if(gameFolder != "Not Existed")
                    {
                        filePath = gameFolder + "\\" + Program.NOL_HD_PROCESS_NAME;
                        if(File.Exists(filePath))
                            this.filePaths.Add(filePath);
                    }
                }
            }
            if (nolClient != null)
                nolClient.Close();
        }
        /* 
         * Module name: button0_Click(...)
         * Author: 张菁菁
         * Purpose: To open number of Nol processes with all requirements set in the main form.
         * Caller: this
         * Note: Need to read all the preferences
         * */
        private void button0_Click(object sender, EventArgs e)
        {// Go!! Button
           // Before open any window, check for existence of "Nobunaga Online Game Mutex" in all opened processes and close it where possible.
            try
            {
                HandleManager.ClearMutex();
            }
            catch (OutOfMemoryException ome)
            {
                MessageBox.Show("Oh no!!为什么又出这个问题。\n对不起，我正在寻找解决这个问题的方法。TAT\nBy 张菁菁\n如果你想知道的话，问题的原因如下(多半你也不想知道0v0)：\n" + ome.StackTrace);
            }
            // Save settings first so this function can use Setting class instead of getting values from the interface.
            this.saveSettings();
            // Get registry key for selected NOL client.
            string nolRegistryEntry = this.getRegistryEntry();
            int[] res = this.GetResolutionOption();
            string resString = (string)Registry.GetValue(nolRegistryEntry+"\\"+Program.NOL_REG_WINDOW_NAME,Program.NOL_REG_RESOLUTION_NAME,null);
            if (resString != null)
            {
                String[] resArr = this.parseResString(resString);
                if (Program.SETTINGS.LastFullScreenToggle)
                    resArr[4] = "0";
                else //
                    resArr[4] = "1";
                resArr[5] = Convert.ToString(res[0]);
                resArr[6] = Convert.ToString(res[1]);
                resArr[8] = Convert.ToString(res[0]);
                resArr[9] = Convert.ToString(res[1]);

                Screen s = Screen.PrimaryScreen;
                if (this.radioButton1.Checked)
                {// 右下
                    int S_width = s.WorkingArea.Width;
                    int S_height = s.WorkingArea.Height;
                    resArr[0] = (S_width - res[0]).ToString();       //startup x
                    resArr[1] = (S_height - res[1]).ToString();      //startup y
                }
                if (this.radioButton2.Checked)
                {// 左上
                    resArr[0] = s.WorkingArea.Left.ToString();      //startup x
                    resArr[1] = s.WorkingArea.Top.ToString(); ;      //startup y
                }
                Process proc = new Process();
                int[] topLeft;
                // Write HD setting to system registry
                if (this.isHighDefinition())
                {
                    setHDSettings(nolRegistryEntry);
                }
                // Write all other settings to system registry
                setOtherSetting(nolRegistryEntry);                                                 
                for (int i = 0; i < Program.SETTINGS.LastNumWnd; i++)
                {
                    if (Program.NOL_MAX_WINDOW_ALLOWED - this.openedProcessId.Count >= 0)
                    {
                        Registry.SetValue(nolRegistryEntry + "\\" + Program.NOL_REG_WINDOW_NAME, Program.NOL_REG_RESOLUTION_NAME, string.Join(",", resArr));
                        proc.StartInfo.FileName = Program.SETTINGS.LastSelectedClientFolder;
                        proc.StartInfo.UseShellExecute = false;
                        proc.Start();
                        
                        Thread.Sleep(Program.SETTINGS.LastDelay);
                        proc.WaitForInputIdle();
                        try
                        {
                            //HandleManager.ClearMutex();
                            HandleManager.CLearMutexInProc(proc);
                            this.openedProcessId.Add(proc.Id);
                        }
                        catch (OutOfMemoryException ome)
                        {
                            MessageBox.Show("Oh no!!为什么又出这个问题。\n对不起，我正在寻找解决这个问题的方法。TAT\nBy 张菁菁\n如果你想知道的话，问题的原因如下(多半你也不想知道0v0)：\n" + ome.StackTrace);
                        }
                        if (Program.SETTINGS.LastAutoDistribution)
                        {
                            topLeft = this.CalculateTopLeftPosition(Program.SETTINGS.LastNumWnd, i, resArr, s);
                            resArr[0] = Convert.ToString(topLeft[0]);
                            resArr[1] = Convert.ToString(topLeft[1]);
                        }
                    }
                }
                if (proc != null)
                    proc.Dispose();
            }
        }
        private void setHDSettings(string nolRegistryEntry)
        {
            string regSettingKey = nolRegistryEntry + "\\" + Program.NOL_REG_SETTING_NAME;
            // Shadow quality
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_HD_SHADOWQUALITY,Program.SETTINGS.LastShadowQuality);
            // Water quality
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_HD_WATERQUALITY, Program.SETTINGS.LastWaterQuality);
            // Lighting quality
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_HD_LIGHTINGQUALITY, Program.SETTINGS.LastLightingQuality);
        }
        private void setOtherSetting(string nolRegistryEntry)
        {
            string regSettingKey = nolRegistryEntry + "\\" + Program.NOL_REG_SETTING_NAME;
            // BGM
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_BGM, ((Program.SETTINGS.LastBGM) ? 1 : 0));
            // SFX
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_SE, ((Program.SETTINGS.LastSFX) ? 1 : 0));
            // OP Movie
            Registry.SetValue(regSettingKey, Program.NOL_REG_SETTING_MOIVE, ((Program.SETTINGS.LastOPMovie) ? 1 : 0));
        }
        private int[] CalculateTopLeftPosition(int numWnd, int currWndNum, String[] resolutionList, Screen s)
        {
            Console.WriteLine(s.DeviceName);
            int[] position = new int[2];
            int x, y;
            x = Int32.Parse(resolutionList[0]);
            y = Int32.Parse(resolutionList[1]);
            int width = Int32.Parse(resolutionList[5]);
            int height = Int32.Parse(resolutionList[6]);
            if (this.radioButton1.Checked && this.radioButton3.Checked)
            {//右下往左上层叠
                int diffFactor_width = (s.WorkingArea.Width - Int32.Parse(resolutionList[5])) / ((numWnd - 1) == 0 ? 1 : (numWnd - 1));
                int diffFactor_Height = (s.WorkingArea.Height - Int32.Parse(resolutionList[6])) / ((numWnd - 1) == 0 ? 1 : (numWnd - 1));
                x -= diffFactor_width;
                y -= diffFactor_Height;
            }
            else if (this.radioButton2.Checked && this.radioButton3.Checked)
            {//左上往右下层叠
                int diffFactor_width = (s.WorkingArea.Width - Int32.Parse(resolutionList[5])) / ((numWnd - 1) == 0 ? 1 : (numWnd - 1));
                int diffFactor_Height = (s.WorkingArea.Height - Int32.Parse(resolutionList[6])) / ((numWnd - 1) == 0 ? 1 : (numWnd - 1));
                x += diffFactor_width;
                y += diffFactor_Height;
            }
            else if (this.radioButton1.Checked && this.radioButton4.Checked)
            {// 右下往左上平铺
                if ((s.WorkingArea.Width / width) >= numWnd)
                    x -= width;
                else
                {
                    int half = 0;
                    int secondHalf = 0;
                    if (numWnd % 2 == 0)
                    {
                        half = numWnd / 2;
                        secondHalf = half;
                    }
                    else
                    {
                        half = numWnd / 2 + 1;
                        secondHalf = numWnd - half;
                    }

                    if (currWndNum < half)
                    {
                        x -= (s.WorkingArea.Width - width) / (half - 1);
                        if (currWndNum + 1 == half)
                        {
                            x = s.WorkingArea.Right - width;
                            y = s.WorkingArea.Top;
                        }
                    }
                    else
                    {
                        x -= (s.WorkingArea.Width - width) / (secondHalf - 1);
                    }
                }
            }
            else if (this.radioButton2.Checked && this.radioButton4.Checked)
            { // 左上往右下平铺
                if ((s.WorkingArea.Width / width) >= numWnd)
                    x += width;
                else
                {
                    int half = 0;
                    int secondHalf = 0;
                    if (numWnd % 2 == 0)
                    {
                        half = numWnd / 2;
                        secondHalf = half;
                    }
                    else
                    {
                        half = numWnd / 2 + 1;
                        secondHalf = numWnd - half;
                    }

                    if (currWndNum < half)
                    {
                        x += (s.WorkingArea.Width - width) / (half - 1);
                        if (currWndNum + 1 == half)
                        {
                            x = s.WorkingArea.Left;
                            y = s.WorkingArea.Top + height;
                        }
                    }
                    else
                    {
                        x += (s.WorkingArea.Width - width) / (secondHalf - 1);
                    }
                }
            }
            Console.WriteLine("x = " + x + "; y=" + y);
            position[0] = x;
            position[1] = y;
            return position;
        }
        private string getRegistryEntry() 
        {
            string gameFolder = Path.GetDirectoryName(Program.SETTINGS.LastSelectedClientFolder);
            string nolRegistryValue = "";
            RegistryKey HKCUSoftware = null;
            RegistryKey koeiKey = null;
            RegistryKey nolKey = null;
            // Open HKEY_CURRENT_USER//Software for search
            HKCUSoftware = Registry.CurrentUser.OpenSubKey(Program.NOL_REG_SOFTWARE,false);
            // Check for TecmoKoei
            koeiKey = HKCUSoftware.OpenSubKey(Program.NOL_REG_NEW_ENTRYPOINT,false);
            string[] subKeyNames;
             
            if (koeiKey != null)
            {
                subKeyNames = koeiKey.GetSubKeyNames();
                for (int i = 0; i < koeiKey.SubKeyCount; i++)
                {
                    if (this.isNolKey(subKeyNames[i]))
                    {
                        nolKey = koeiKey.OpenSubKey(subKeyNames[i]);
                        string tempGameFolder = "";
                        tempGameFolder = (string)nolKey.GetValue(Program.NOL_REG_GAMEFOLDER_NAME,"Not exsits");
                        if (tempGameFolder != "Not exsits" && 
                            tempGameFolder == gameFolder)
                        {
                            nolRegistryValue = nolKey.ToString();
                        }
                    }
                }
            }
            // Check for KOEI
            koeiKey = HKCUSoftware.OpenSubKey(Program.NOL_REG_OLD_ENTRYPOINT, false);
            if (koeiKey != null)
            {
                subKeyNames = koeiKey.GetSubKeyNames();
                for (int i = 0; i < koeiKey.SubKeyCount; i++)
                {
                    if (this.isNolKey(subKeyNames[i]))
                    {
                        nolKey = koeiKey.OpenSubKey(subKeyNames[i]);
                        string tempGameFolder = "";
                        tempGameFolder = (string)nolKey.GetValue(Program.NOL_REG_GAMEFOLDER_NAME, "Not exsits");
                        if (tempGameFolder != "Not exsits" &&
                            tempGameFolder == gameFolder)
                        {
                            nolRegistryValue = nolKey.ToString();
                        }
                    }
                }
            }
            if(HKCUSoftware != null)
                HKCUSoftware.Close();
            if(koeiKey != null)
                koeiKey.Close();
            if(nolKey != null)
                nolKey.Close();
            return nolRegistryValue; // if correct it should be something like HKEY_CURRENT_USER//Software//TecmoKoei
        }
        private bool isNolKey(string regKeyName) 
        {
            for (int i = 0; i < Program.NOL_REG_CLIENTS_SD.Length; i++)
            {
                if (regKeyName == Program.NOL_REG_CLIENTS_SD[i])
                    return true;
            }
            for (int i = 0; i < Program.NOL_REG_CLIENTS_HD.Length; i++)
            {
                if (regKeyName == Program.NOL_REG_CLIENTS_HD[i])
                    return true;
            }
            return false;
        }
        /* 
         * Module name: isHighDefinition()
         * Author: 张菁菁
         * Purpose: Check if the selected folder leads to a HD or SD version of Nobunaga Online client
         * Caller: button0_Click(...)
         * Note: Similar to toggleHDSettings(), but it won't touch any interface methods.
         */
        private bool isHighDefinition()
        {
            string filePath = (string)this.comboBox1.SelectedItem;
            string gameFolder = Path.GetDirectoryName(filePath);
            return (File.Exists(gameFolder + "\\" + Program.NOL_HD_PROCESS_NAME) &&
                   File.Exists(gameFolder + "\\" + Program.NOL_HD_LAUNCHER_NAME));
        }
        /* Module name: parseResString(...)
         * Author: 张菁菁
         * Purpose: parse Koei Nobunaga Online Tc resolution string from its registry.
         * Caller: button0_Click(...)
         * Note1: 
         * resolution string explained
         * example 184,158,1,0,1,800,600,1,800,600,0,22
         * [00]184  left: start-up x-coordinate 
         * [01]158  top:  start-up y-coordinate 
         * [02]1    don't care / don't know
         * [03]0    don't care / don't know
         * [04]1    windowed mode toggle. 1 = windowed mode, 0 = full screen
         * [05]800  resolution: width windowed mode (not tested, can be full screen)
         * [06]600  resolution height windowed mode (not tested, can be full screen)
         * [07]1    don't care
         * [08]800  resolution width full screen (not tested, can be windowed mode)
         * [09]600  resolution width full screen (not tested, can be windowed mode)
         * [10]0    don't care / don't know
         * [11]22   don't care / don't know
         * 
         * Note2:
         * Tests show that [05] [06] and [08] [09] can be different, further test required.
         * for the time being, both of them will be set to the same resolution for this program.
         */
        private string[] parseResString(string resString)
        {
            char[] delim = { ',' };
            return resString.Split(delim);
        }
        private int[] GetResolutionOption()
        {
            int[] resolution = new int[2];
            string[] resArr = Program.SETTINGS.LastResolution.Split('x');//this.comboBox2.Text.Split('x');
            resolution[0] = Int32.Parse(resArr[0].Trim());
            resolution[1] = Int32.Parse(resArr[1].Trim());
            return resolution;
        }
        private void button2_Click(object sender, EventArgs e)
        {// 队伍状态 button
            if (Form2.Exists)
                return;
            else
            {
                button2.Enabled = false;
                form2 = new Form2(this.openedProcessId);
                form2.FormClosed += new FormClosedEventHandler(
                    delegate(object _sender, FormClosedEventArgs _e) 
                    {
                        button2.Enabled = true;
                    });
                form2.StartPosition = FormStartPosition.Manual;
                form2.Location = new Point(this.Location.X+this.Width,this.Location.Y);
                form2.Show();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {//退出 button
            Application.Exit();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "打开客户端目录...";
            this.openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            this.openFileDialog1.FileName = "*.bng";
            this.openFileDialog1.Filter = "bng file (*.bng)|*.bng|All files (*.*)|*.*";
            this.openFileDialog1.FilterIndex = 1;
            this.openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int idx = this.filePaths.FindIndex(fp => fp == openFileDialog1.FileName); // Check if the file path has already been added to the list.
                if (idx >= 0)
                {
                    this.comboBox1.SelectedIndex = idx;
                    this.toolStripStatusLabel1.Text = "文件已存在，请使用下拉菜单选取。";
                }
                else 
                {
                    this.filePaths.Add(openFileDialog1.FileName);
                    this.comboBox1.Items.Insert(0, openFileDialog1.FileName);
                    this.comboBox1.SelectedIndex = 0;
                    this.toolStripStatusLabel1.Text = "文件已添加，请使用下拉菜单选取。";
                }
            }
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton6.Checked)
            {
                this.numericUpDown1.Value = 1;
                this.numericUpDown1.Enabled = false;
                this.groupBox1.Enabled = false;
                string filePath = (string)this.comboBox1.SelectedItem;
                string gameFolder = Path.GetDirectoryName(filePath);
                this.changeResolutions(gameFolder);  
            }
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton5.Checked)
            {
                this.numericUpDown1.Enabled = true;
                this.groupBox1.Enabled = true;
                this.checkBox1.Checked = true;
                this.radioButton1.Checked = true;
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = true;
                this.radioButton4.Checked = false;
                string filePath = (string)this.comboBox1.SelectedItem;
                string gameFolder = Path.GetDirectoryName(filePath);
                this.changeResolutions(gameFolder);  
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filePath = (string)this.comboBox1.SelectedItem;
            string gameFolder = Path.GetDirectoryName(filePath);
            this.toggleHDSettings(gameFolder);            
        }
        private void changeResolutions(string gameFolder)
        {
            /* Check if selected file path leads to SD version or HD version
             * User may select a custom game executable for some reason, 
             * so check in the game folder for official game executable like "nobolHD.exe" or "nobolHD.bng"
             */
            if (File.Exists(gameFolder + "\\" + Program.NOL_HD_PROCESS_NAME) &&
                    File.Exists(gameFolder + "\\" + Program.NOL_HD_LAUNCHER_NAME))
            {
                if (this.radioButton5.Checked)
                {// load HD windowed resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_HD_WINDOWED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
                else if (this.radioButton6.Checked)
                {// load HD fullscreen resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_HD_FULLSCREENED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
            }
            else if (File.Exists(gameFolder + "\\" + Program.NOL_SD_PROCESS_NAME) &&
                     File.Exists(gameFolder + "\\" + Program.NOL_SD_LAUNCHER_NAME))
            {
                if (this.radioButton5.Checked)
                {// load SD windowed resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_SD_WINDOWED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
                else if (this.radioButton6.Checked)
                {// load SD fullscreen resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_SD_FULLSCREENED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
            }
        }
        private void toggleHDSettings(string gameFolder)
        {
            /* Check if selected file path leads to SD version or HD version
             * User may select a custom game executable for some reason, 
             * so check in the game folder for official game executable like "nobolHD.exe" or "nobolHD.bng"
             */
            if (File.Exists(gameFolder + "\\" + Program.NOL_HD_PROCESS_NAME) &&
                    File.Exists(gameFolder + "\\" + Program.NOL_HD_LAUNCHER_NAME))
            {// Selected folder is a HD version 
                groupBox3.Enabled = true;
                radioButton7.Enabled = true;
                radioButton8.Enabled = true;
                radioButton9.Enabled = true;
                radioButton10.Enabled = true;
                radioButton11.Enabled = true;
                radioButton12.Enabled = true;
                radioButton13.Enabled = true;
                radioButton14.Enabled = true;
                radioButton15.Enabled = true;
                radioButton16.Enabled = true;
                radioButton17.Enabled = true;
                radioButton18.Enabled = true;
                this.toolStripStatusLabel1.Text = "高清版信长，可进行高清设置。";
                if (this.radioButton5.Checked)
                {// load HD windowed resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_HD_WINDOWED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
                else if (this.radioButton6.Checked)
                {// load HD fullscreen resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_HD_FULLSCREENED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
            }
            else if (File.Exists(gameFolder + "\\" + Program.NOL_SD_PROCESS_NAME) &&
                     File.Exists(gameFolder + "\\" + Program.NOL_SD_LAUNCHER_NAME))
            {// Selected folder is a SD version
                // Uncheck all radio buttons in the group panel (7 to 18)
                radioButton7.Checked = false;
                radioButton8.Checked = false;
                radioButton9.Checked = false;
                radioButton10.Checked = false;
                radioButton11.Checked = false;
                radioButton12.Checked = false;
                radioButton13.Checked = false;
                radioButton14.Checked = false;
                radioButton15.Checked = false;
                radioButton16.Checked = false;
                radioButton17.Checked = false;
                radioButton18.Checked = false;

                // Disable all radio buttons in the group panel (7 to 18)
                radioButton7.Enabled = false;
                radioButton8.Enabled = false;
                radioButton9.Enabled = false;
                radioButton10.Enabled = false;
                radioButton11.Enabled = false;
                radioButton12.Enabled = false;
                radioButton13.Enabled = false;
                radioButton14.Enabled = false;
                radioButton15.Enabled = false;
                radioButton16.Enabled = false;
                radioButton17.Enabled = false;
                radioButton18.Enabled = false;
                
                // Disable the group panel as well
                groupBox3.Enabled = false;
                // Status
                this.toolStripStatusLabel1.Text = "标清版信长，高清设置不可用。";
                if (this.radioButton5.Checked)
                {// load SD windowed resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_SD_WINDOWED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
                else if (this.radioButton6.Checked)
                {// load SD fullscreen resolutions to combobox2
                    this.comboBox2.Items.Clear();
                    this.comboBox2.Items.AddRange(Program.NOL_SD_FULLSCREENED_RESOLUTIONS);
                    this.comboBox2.SelectedIndex = 0;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.statusStrip1.Text = "窗口关闭中...";
            DialogResult dialogResult = MessageBox.Show("关闭本程序将关闭所有本程序打开的信长窗口\n请先在信长中下线以免造成游戏资料错误。\n\n按“确定”关闭本程序或按“取消”返回主窗口。","请注意！", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation,MessageBoxDefaultButton.Button2);
            if (dialogResult != DialogResult.OK)
                e.Cancel = true;
            else
            {
                this.closeNOLProcessesOpenedByMe();
                this.openedProcessId.Clear();
                this.saveSettings();
                this.statusStrip1.Text = "确认关闭。";
            }
        }
        private void closeNOLProcessesOpenedByMe() 
        {
            Process proc = null;
            for (int i = 0; i < this.openedProcessId.Count; i++)
            {
                if (Helper.IsRunning(this.openedProcessId[i]))
                {
                    proc = Process.GetProcessById(this.openedProcessId[i]);
                    try
                    {
                        HandleManager.TerminateProcessByHandle(proc.Handle);
                    }
                    catch (Exception)
                    {
                        proc.Dispose();
                    }
                }
            }
            if (proc != null)
                proc.Dispose();
        }
        private void loadSettings(string fileName)
        {
            Program.SETTINGS = Program.SETTINGS.ReadFromFile(fileName);
            for (int i = 0; i < Program.SETTINGS.LastLoadedClientFolders.Count; i++)
            {
                if (!this.comboBox1.Items.Contains(Program.SETTINGS.LastLoadedClientFolders[i]))
                {
                    this.comboBox1.Items.Add(Program.SETTINGS.LastLoadedClientFolders[i]);
                }
            }
            this.comboBox1.SelectedIndex = this.comboBox1.Items.IndexOf(Program.SETTINGS.LastSelectedClientFolder);
            this.numericUpDown1.Value = Program.SETTINGS.LastNumWnd;
            this.numericUpDown2.Value = Program.SETTINGS.LastDelay;
            if (Program.SETTINGS.LastFullScreenToggle)
                this.radioButton6.Checked = true;
            else
                this.radioButton5.Checked = true;
            this.checkBox2.Checked = Program.SETTINGS.LastBGM;
            this.checkBox3.Checked = Program.SETTINGS.LastSFX;
            this.checkBox4.Checked = Program.SETTINGS.LastOPMovie;
            if (this.comboBox2.Items.Count > 0)
                this.comboBox2.SelectedIndex = this.comboBox2.Items.IndexOf(Program.SETTINGS.LastResolution);
            switch(Program.SETTINGS.LastShadowQuality)
            {
                case 0:
                    this.radioButton7.Checked = true;
                    break;
                case 1:
                    this.radioButton8.Checked = true;
                    break;
                case 2:
                    this.radioButton9.Checked = true;
                    break;
                case 3:
                    this.radioButton10.Checked = true;
                    break;
                default:
                    this.radioButton8.Checked = true;
                    break;
            }
            switch (Program.SETTINGS.LastWaterQuality)
            {
                case 0:
                    this.radioButton14.Checked = true;
                    break;
                case 1:
                    this.radioButton13.Checked = true;
                    break;
                case 2:
                    this.radioButton12.Checked = true;
                    break;
                case 3:
                    this.radioButton11.Checked = true;
                    break;
                default:
                    this.radioButton13.Checked = true;
                    break;
            }
            switch (Program.SETTINGS.LastLightingQuality)
            {
                case 0:
                    this.radioButton18.Checked = true;
                    break;
                case 1:
                    this.radioButton17.Checked = true;
                    break;
                case 2:
                    this.radioButton16.Checked = true;
                    break;
                case 3:
                    this.radioButton15.Checked = true;
                    break;
                default:
                    this.radioButton17.Checked = true;
                    break;
            }
            this.checkBox1.Checked = Program.SETTINGS.LastAutoDistribution;
            if (!this.checkBox1.Checked)
            {
                this.radioButton1.Checked = false;
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = false;
                this.radioButton4.Checked = false;
            }
            switch (Program.SETTINGS.LastDistributionMethod)
            { 
                case 0:
                    this.radioButton1.Checked = true;
                    break;
                case 1:
                    this.radioButton2.Checked = true;
                    break;
                default:
                    this.radioButton1.Checked = true;
                    break;
            }
            switch (Program.SETTINGS.LastWindowAlignment)
            {
                case 0:
                    this.radioButton3.Checked = true;
                    break;
                case 1:
                    this.radioButton4.Checked = true;
                    break;
                default:
                    this.radioButton3.Checked = true;
                    break;
            }
        }
        private void saveSettings() 
        {
            Program.SETTINGS.LastSelectedClientFolder = this.comboBox1.Text;
            for (int i = 0; i < this.comboBox1.Items.Count; i++)
            {
                Program.SETTINGS.LastLoadedClientFolders.Add((string)this.comboBox1.Items[i]);
            }
            Program.SETTINGS.LastNumWnd = (int)this.numericUpDown1.Value;
            Program.SETTINGS.LastDelay = (int)this.numericUpDown2.Value;
            if (this.radioButton5.Checked)
                Program.SETTINGS.LastFullScreenToggle = false;
            else if (this.radioButton6.Checked)
                Program.SETTINGS.LastFullScreenToggle = true;
            Program.SETTINGS.LastBGM = this.checkBox2.Checked;
            Program.SETTINGS.LastSFX = this.checkBox3.Checked;
            Program.SETTINGS.LastOPMovie = this.checkBox4.Checked;
            Program.SETTINGS.LastResolution = this.comboBox2.Text;
            // Shadow quality
            if (this.radioButton7.Checked) Program.SETTINGS.LastShadowQuality = 0;
            else if (this.radioButton8.Checked) Program.SETTINGS.LastShadowQuality = 1;
            else if (this.radioButton9.Checked) Program.SETTINGS.LastShadowQuality = 2;
            else /*if (this.radioButton10.Checked)*/ Program.SETTINGS.LastShadowQuality = 3;
            // Water quality
            if (this.radioButton14.Checked) Program.SETTINGS.LastWaterQuality = 0;
            else if (this.radioButton13.Checked) Program.SETTINGS.LastWaterQuality = 1;
            else if (this.radioButton12.Checked) Program.SETTINGS.LastWaterQuality = 2;
            else /*if (this.radioButton11.Checked)*/ Program.SETTINGS.LastWaterQuality = 3;
            // Lighting quality
            if (this.radioButton18.Checked) Program.SETTINGS.LastLightingQuality = 0;
            else if (this.radioButton17.Checked) Program.SETTINGS.LastLightingQuality = 1;
            else if (this.radioButton16.Checked) Program.SETTINGS.LastLightingQuality = 2;
            else /*if (this.radioButton15.Checked)*/ Program.SETTINGS.LastLightingQuality = 3;
            Program.SETTINGS.LastAutoDistribution = this.checkBox1.Checked;
            if (this.radioButton1.Checked)
                Program.SETTINGS.LastDistributionMethod = 0;
            else if(this.radioButton2.Checked)
                Program.SETTINGS.LastDistributionMethod = 1;
            if (this.radioButton3.Checked)
                Program.SETTINGS.LastWindowAlignment = 0;
            else if (this.radioButton4.Checked)
                Program.SETTINGS.LastWindowAlignment = 1;

            string pathToSave = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fileName = Path.Combine(pathToSave, Program.SETTINGS_FILENAME);
            Console.WriteLine(fileName);
            Program.SETTINGS.SaveToFile(fileName);
        }
        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                this.panel1.Enabled = true;
                this.panel2.Enabled = true;
                this.radioButton1.Checked = true;
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = true;
                this.radioButton4.Checked = false;
            }
            else 
            {
                this.panel1.Enabled = false;
                this.panel2.Enabled = false;
                this.radioButton1.Checked = false;
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = false;
                this.radioButton4.Checked = false;
            }
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if(Form2.Exists)
                form2.Location = new Point(this.Location.X + this.Width, this.Location.Y);
        }

        private void groupBox3_EnabledChanged(object sender, EventArgs e)
        {
            if(!this.groupBox3.Enabled)
            {
                radioButton7.Checked = false;
                radioButton8.Checked = false;
                radioButton9.Checked = false;
                radioButton10.Checked = false;
                radioButton11.Checked = false;
                radioButton12.Checked = false;
                radioButton13.Checked = false;
                radioButton14.Checked = false;
                radioButton15.Checked = false;
                radioButton16.Checked = false;
                radioButton17.Checked = false;
                radioButton18.Checked = false;
            }
        }

        private void groupBox1_EnabledChanged(object sender, EventArgs e)
        {
            if (!this.groupBox1.Enabled)
            {
                this.checkBox1.Checked = false;
                this.radioButton1.Checked = false;
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = false;
                this.radioButton4.Checked = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("即将关闭所有打开的信长窗口\n请先在信长中下线以免造成游戏资料错误。\n\n按“确定”关闭本程序或按“取消”返回主窗口。", "请注意！", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (dialogResult == DialogResult.OK)
            {
                HandleManager.CloseAllWindow();
                this.openedProcessId.Clear();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach(int pid in this.openedProcessId)
            {
                Process proc = Process.GetProcessById(pid);
                IntPtr hWnd = proc.Handle;
                Console.WriteLine(pid);
                Console.WriteLine(HandleManager.MinimizeWindow(hWnd));
            }
        }

    }
}
