using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
namespace NolMultiLauncherAllServer
{
    public class Settings
    {
        public string LastSelectedClientFolder { get; set; }
        public List<string> LastLoadedClientFolders = new List<string>();
        private int lastNumWnd;
        private int lastDelay;
        private int lastShadowQuality;
        private int lastWaterQuality;
        private int lastLightingQuality;
        private int lastDistributionMethod;
        private int lastWindowAlignment;
        public int LastNumWnd 
        {
            get { return lastNumWnd; }
            set 
            {
                if(value > 0 && value < 8 )
                    lastNumWnd = value;
                if (value > 7) lastNumWnd = 7;
                if (value < 1) lastNumWnd = 1;
            }
        }
        public int LastDelay
        {
            get { return lastDelay; }
            set 
            {
                lastDelay = value;
                if (value > 10000) lastDelay = 10000;
                if (value < 0) lastDelay = 0;
            }
        }
        public bool LastFullScreenToggle { get; set; } // Windowed = false, Full Screen = true
        public bool LastBGM { get; set; } // enable = true， disable = false
        public bool LastSFX { get; set; } // enable = true, disable = false
        public bool LastOPMovie { get; set; } // enable = true, disable = false
        public string LastResolution { get; set; }
        public int LastShadowQuality // low = 0, medium = 1, high = 2, ultra = 3
        {
            get { return lastShadowQuality; }
            set 
            {
                lastShadowQuality = value;
                if (value > 3) lastShadowQuality = 3;
                if (value < 0) lastShadowQuality = 0;
            }
        }
        public int LastWaterQuality // low = 0, medium = 1, high = 2, ultra = 3
        {
            get { return lastWaterQuality; }
            set
            {
                lastWaterQuality = value;
                if (value > 3) lastWaterQuality = 3;
                if (value < 0) lastWaterQuality = 0;
            }
        }
        public int LastLightingQuality // low = 0, medium = 1, high = 2, ultra = 3
        {
            get { return lastLightingQuality; }
            set
            {
                lastLightingQuality = value;
                if (value > 3) lastLightingQuality = 3;
                if (value < 0) lastLightingQuality = 0;
            }
        }
        public bool LastAutoDistribution { get; set; }
        public int LastDistributionMethod // Start from bottom right = 0, start form top left = 1
        {
            get { return lastDistributionMethod; }
            set
            {
                lastDistributionMethod = value;
                if (value > 1) lastDistributionMethod = 1;
                if (value < 0) lastDistributionMethod = 0;
            }
        }
        public int LastWindowAlignment  // Tile = 1, Cascading = 0;
        {
            get { return lastWindowAlignment; }
            set 
            {
                lastWindowAlignment = value;
                if (value > 1) lastWindowAlignment = 1;
                if (value < 0) lastWindowAlignment = 0;
            }
        }
        private static XmlSerializer xs;
        static Settings()
        {
            xs = new XmlSerializer(typeof(Settings));
        }
        public void SaveToFile(string fileName)
        {
            using(StreamWriter sw = new StreamWriter(fileName))
            {
                xs.Serialize(sw, this);
            }
        }

        public Settings ReadFromFile(string fileName)
        {
            using(StreamReader sr = new StreamReader(fileName))
            {
                return xs.Deserialize(sr) as Settings;
            }
        }
    }
}
