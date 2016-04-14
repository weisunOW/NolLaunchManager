using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolMultiLauncherAllServer
{
    class Player
    {
        private int hProcess;
        private string surname;
        private string firstname;
        private int x;
        private int y;
        private int z;
        private int orientation;
        private string username;
        private string password;
        private double moneyAtHand;
        private double moneyInBank;
        private double moneyInCommonBank;
        private List<int> itemList;
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }
        public int Z
        {
            get { return this.x; }
            set { this.y = value; }
        }
        public int Orientation
        {
            get { return this.orientation; }
            set { this.orientation = value; }
        }
        public string Username
        {
            get { return this.username; }
            set { this.username = value; }
        }
        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }
        public double MoneyAtHand
        {
            get { return this.moneyAtHand; }
            set { this.moneyAtHand = value; }
        }
        public double MoneyInBank
        {
            get { return this.moneyInBank; }
            set { this.moneyInBank = value; }
        }
        public double MoneyInCommonBank
        {
            get { return this.moneyInCommonBank; }
            set { this.moneyInCommonBank = value; }
        }
        public List<int> ItemList
        {
            get { return this.itemList; }
            set { this.itemList = value; }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Process Handle: ").Append(this.hProcess).Append("\n").Append("Surname: ").Append(this.surname).Append("\n").Append("Firstname: ").Append(this.firstname).Append("\n");
            return sb.ToString();
        }
        public Player()
        {
            this.hProcess = 0;
            this.surname = "";
            this.firstname = "";
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.orientation = 0;
            this.username = "";
            this.password = "";
            this.moneyAtHand = 0.0;
            this.moneyInBank = 0.0;
            this.moneyInCommonBank = 0.0;
            this.itemList = new List<int>();
        }
        public Player(int hProc, string sname, string fname, int x, int y, int z, int ori, string uname, string pword, double mah, double mib, double micb) 
        {
            this.hProcess = hProc;
            this.surname = sname;
            this.firstname = fname;
            this.x = x;
            this.y = y;
            this.z = z;
            this.orientation = ori;
            this.username = uname;
            this.password = pword;
            this.moneyAtHand = mah;
            this.moneyInBank = mib;
            this.moneyInCommonBank = micb;
            this.itemList = new List<int>();
        }
        
    }
}
