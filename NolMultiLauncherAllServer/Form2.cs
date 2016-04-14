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

namespace NolMultiLauncherAllServer
{
    public partial class Form2 : Form, IDisposable
    {
        private List<int> nolOpenedWindowHandles;
        public Form2()
        {
            InitializeComponent();
            form2Disposed = false;
            bool owned;
            form2Mutex = new Mutex(true, FORM2_MUTEX_NAME, out owned);
            if (!owned)
            {
                Close();
                return;
            }
            nolOpenedWindowHandles = new List<int>();
        }
        public Form2(List<int> procList)
        {
            InitializeComponent();
            form2Disposed = false;
            bool owned;
            form2Mutex = new Mutex(true, FORM2_MUTEX_NAME, out owned);
            if (!owned)
            {
                Close();
                return;
            }
            nolOpenedWindowHandles = new List<int>(procList);
            this.initProcListView();
        }
        public void initProcListView()
        {
            if (this.nolOpenedWindowHandles.Count > 0)
            {
                for (int i = 0; i < this.nolOpenedWindowHandles.Count; i++)
                {
                    this.listBox1.Items.Add(this.nolOpenedWindowHandles[i]);
                }
                this.listBox1.SelectedIndex = 0;
            }
        }
        ~Form2()
        {
            Dispose(false);
        }
        // IDisposable Members
        // --------------------------------------------------------------------------
        #region IDisposable Members
        new public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }
        #endregion  // IDisposable Members
        static public bool Exists 
        {
            get 
            {
                bool owned;
                using (new Mutex(false, FORM2_MUTEX_NAME, out owned))
                {
                    return !owned;
                }
            }
        }
        public List<int> NolOpenedWindowHandles
        {
            get { return this.nolOpenedWindowHandles; }
            set { nolOpenedWindowHandles = value; }
        }

        private void Form2_LocationChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.nolOpenedWindowHandles.Count > 0 && (this.listBox1.SelectedIndex >= 0 || this.listBox1.SelectedIndex <= this.listBox1.Items.Count))
            {
                int index = this.listBox1.SelectedIndex;
                int procId = this.nolOpenedWindowHandles[index];
                Process proc = Process.GetProcessById(procId);
                IntPtr handle = proc.Handle;
                Console.WriteLine(handle);
                byte[] sBuffer = Helper.ReadMemory(handle, (int)MemoryLocations.NOL_SD_MEM_LOCATIONS.NOL_SD_MEM_SURENAME, (int)MemoryLocations.NOL_SD_MEM_LENGTH.NOL_SD_MEM_SURENAME_LENGTH);
                byte[] fBuffer = Helper.ReadMemory(handle, (int)MemoryLocations.NOL_SD_MEM_LOCATIONS.NOL_SD_MEM_FIRSTNAME, (int)MemoryLocations.NOL_SD_MEM_LENGTH.NOL_SD_MEM_FIRSTNAME_LENGTH);
                Console.WriteLine("姓名缓存长度: " + sBuffer.Length + " " + fBuffer.Length);
                string surname = Encoding.Unicode.GetString(sBuffer);
                string firstname = Encoding.Unicode.GetString(fBuffer);
                Console.WriteLine("姓名："+surname+firstname);
                this.label9.Text = surname + firstname;
            }
        }
    }
}
