/*Nobunaga's Ambition Online MultiLauncher (信长之野望 Online 多开辅助工具)
 *The Nobunaga's Ambition Online's executable is never modified.
 *This program can be use for client applications connecting to all servers including
 *  1) Japan
 *  2) Taiwan
 *  3) and upcoming Chinese Mainland server
 * 
 *Copyright (C) 2013 张菁菁@烧津海贼众

 *This program is free software: you can redistribute it and/or modify
 *it under the terms of the GNU General Public License as published by
 *the Free Software Foundation, either version 3 of the License, or
 *(at your option) any later version.

 *This program is distributed in the hope that it will be useful,
 *but WITHOUT ANY WARRANTY; without even the implied warranty of
 *MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *GNU General Public License for more details.

 *You should have received a copy of the GNU General Public License
 *along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace NolMultiLauncherAllServer
{
    static class Program
    {
        // Global variable for Nolbunaga Online Registry entry.
        public static int NOL_MAX_WINDOW_ALLOWED = 7;
        public static string NOL_REG_HKEY_CURRENT_USER = "HKEY_CURRENT_USER";
        public static string NOL_REG_SOFTWARE = "Software"; 
        public static string NOL_REG_NEW_ENTRYPOINT = "TecmoKoei";
        public static string NOL_REG_OLD_ENTRYPOINT = "KOEI";
        public static string NOL_REG_KEY_JAPAN_SD = "Nobunaga Online";
        public static string NOL_REG_KEY_JAPAN_HD = "Nobunaga Online HD";
        public static string NOL_REG_KEY_TAIWAN_SD = "Nobunaga Online Tc";
        public static string NOL_REG_KEY_TAIWAN_HD = "Nobunaga Online HD Tc";
        public static string NOL_REG_KEY_MAINLAND_SD = "Nobunaga Online Sc"; // I haven't seen the new client application for mainland server, this is subjected to change.
        public static string NOL_REG_KEY_MAINLAND_HD = "Nobunaga Online HD Sc"; // as above
        public static string NOL_MUTEX_NAME = "Nobunaga Online Game Mutex";
        public static string NOL_SD_PROCESS_NAME = "nobol.bng";
        public static string NOL_HD_PROCESS_NAME = "nobolHD.bng";
        public static string NOL_SD_LAUNCHER_NAME = "nobol.exe";
        public static string NOL_HD_LAUNCHER_NAME = "nobolHD.exe";
        public static string NOL_MOVIE_OVERLAY = "nobol.bnm";
        public static string NOL_REG_WINDOW_NAME = "Window";
        public static string NOL_REG_GAMEFOLDER_NAME = "GameFolder";
        public static string NOL_REG_RESOLUTION_NAME = "DeviceRequirement"; // Resolution and full screen toggle
        public static string NOL_REG_SETTING_NAME = "Setting";
        public static string NOL_REG_SETTING_PLAYSOUNDNOACTIVE = "PlaySoundNoActive"; // Play BGM when game window is not in focus.
        public static string NOL_REG_SETTING_BGM = "BGM"; // BGM
        public static string NOL_REG_SETTING_SE = "SE"; // Sound Effect
        public static string NOL_REG_SETTING_MOIVE = "Movie"; // Openning movie
        public static string NOL_REG_SETTING_HD_LIGHTINGQUALITY = "LightingQuality";
        public static string NOL_REG_SETTING_HD_SHADOWQUALITY = "ShadowQuality";
        public static string NOL_REG_SETTING_HD_WATERQUALITY = "WaterQuality";
        public static string[] NOL_ENTRYPOINTS = { 
                                                      NOL_REG_NEW_ENTRYPOINT,
                                                      NOL_REG_OLD_ENTRYPOINT
                                                 };
        public static string[] NOL_PROCESS_NAMES = {
                                                       NOL_HD_PROCESS_NAME,
                                                       NOL_SD_PROCESS_NAME
                                                   };
        public static string[] NOL_REG_SETTING_HD_QUALITIES = {
                                                                  NOL_REG_SETTING_HD_LIGHTINGQUALITY,
                                                                  NOL_REG_SETTING_HD_SHADOWQUALITY,
                                                                  NOL_REG_SETTING_HD_WATERQUALITY
                                                              };
        public static string[] NOL_REG_SETTING_OTHERS = {
                                                            NOL_REG_SETTING_BGM,
                                                            NOL_REG_SETTING_SE,
                                                            NOL_REG_SETTING_MOIVE,
                                                            NOL_REG_SETTING_PLAYSOUNDNOACTIVE
                                                        };
        public static string[] NOL_REG_CLIENTS_SD = {
                                                        NOL_REG_KEY_JAPAN_SD,
                                                        NOL_REG_KEY_TAIWAN_SD,
                                                        NOL_REG_KEY_MAINLAND_SD
                                                    };
        public static string[] NOL_REG_CLIENTS_HD = {
                                                        NOL_REG_KEY_JAPAN_HD,
                                                        NOL_REG_KEY_TAIWAN_HD,
                                                        NOL_REG_KEY_MAINLAND_HD
                                                    };
        public static Settings SETTINGS = new Settings();
        public static string SETTINGS_FILENAME = "xmlNolMultilauncherSettings.xml";
        public static string[] NOL_SD_WINDOWED_RESOLUTIONS = {
                                                        "640 x 480",
                                                        "720 x 480",
                                                        "720 x 576",
                                                        "800 x 600",
                                                        "1024 x 768",
                                                        "1152 x 864",
                                                        "1176 x 664",
                                                        "1280 x 720",
                                                        "1280 x 768",
                                                        "1280 x 800",
                                                        "1280 x 960",
                                                        "1280 x 1024",
                                                        "1360 x 768",
                                                        "1366 x 768",
                                                        "1440 x 900",
                                                        "1600 x 900",
                                                        "1600 x 1024"
                                                    };
        public static string[] NOL_HD_WINDOWED_RESOLUTIONS = {
                                                        "640 x 480",
                                                        "720 x 480",
                                                        "720 x 576",
                                                        "800 x 600",
                                                        "1024 x 768",
                                                        "1152 x 864",
                                                        "1176 x 664",
                                                        "1280 x 720",
                                                        "1280 x 768",
                                                        "1280 x 800",
                                                        "1280 x 960",
                                                        "1280 x 1024",
                                                        "1360 x 768",
                                                        "1366 x 768",
                                                        "1440 x 900",
                                                        "1600 x 900",
                                                        "1600 x 1024",
                                                        "1768 x 992"
                                                    };
        public static string[] NOL_SD_FULLSCREENED_RESOLUTIONS = {
                                                        "640 x 480",
                                                        "720 x 480",
                                                        "720 x 576",
                                                        "800 x 600",
                                                        "1024 x 768",
                                                        "1152 x 864",
                                                        "1176 x 664",
                                                        "1280 x 720",
                                                        "1280 x 768",
                                                        "1280 x 800",
                                                        "1280 x 960",
                                                        "1280 x 1024",
                                                        "1360 x 768",
                                                        "1366 x 768",
                                                        "1440 x 900",
                                                        "1600 x 900",
                                                        "1600 x 1024",
                                                        "1600 x 1200"
                                                    };
        public static string[] NOL_HD_FULLSCREENED_RESOLUTIONS = {
                                                        "640 x 480",
                                                        "720 x 480",
                                                        "720 x 576",
                                                        "800 x 600",
                                                        "1024 x 768",
                                                        "1152 x 864",
                                                        "1176 x 664",
                                                        "1280 x 720",
                                                        "1280 x 768",
                                                        "1280 x 800",
                                                        "1280 x 960",
                                                        "1280 x 1024",
                                                        "1360 x 768",
                                                        "1366 x 768",
                                                        "1440 x 900",
                                                        "1600 x 900",
                                                        "1600 x 1024",
                                                        "1600 x 1200",
                                                        "1680 x 1050",
                                                        "1768 x 992",
                                                        "1920 x 1080"
                                                    };
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
