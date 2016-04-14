using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolMultiLauncherAllServer
{
    class MemoryLocations
    {
        // Memory Address
        public enum NOL_SD_MEM_LOCATIONS : int
        {
            NOL_SD_MEM_USERNAME = 0x9FF76C,
            NOL_SD_MEM_PASSWORD = 0x9FF781,
            NOL_SD_MEM_X = 0x9FF424,
            NOL_SD_MEM_Y = 0x9FF42C,
            NOL_SD_MEM_Z = 0x9FF428,
            NOL_SD_MEM_SURENAME = 0x9FF15C,
            NOL_SD_MEM_FIRSTNAME = 0x9FF16E,
            NOL_SD_MEM_CURRENT_MONEY_INTEGER = 0xA08CDC,
            NOL_SD_MEM_CURRENT_MONEY_DECIMAL = 0xA08CE0,
            NOL_SD_MEM_CURRENT_MONEY_BANK_INTEGER = 0xA0B588,
            NOL_SD_MEM_CURRENT_MONEY_BANK_DECIMAL = 0xA0B58c,
            NOL_SD_MEM_CURRENT_MONEY_COMMON_BANK = 0xA0B590
        }
        // Memory Allocation
        // number of bytes to read
        public enum NOL_SD_MEM_LENGTH : int
        { 
            NOL_SD_MEM_USERNAME_LENGTH = 20,
            NOL_SD_MEM_PASSWORD_LENGTH = 20,
            NOL_SD_MEM_Z_LENGTH = 4,
            NOL_SD_MEM_Y_LENGTH = 4,
            NOL_SD_MEM_X_LENGTH = 4,
            NOL_SD_MEM_SURENAME_LENGTH = 6,
            NOL_SD_MEM_FIRSTNAME_LENGTH = 6,
            NOL_SD_MEM_CURRENT_MONEY_INTEGER_LENGTH = 4,
            NOL_SD_MEM_CURRENT_MONEY_DECIMAL_LENGTH = 2,
            NOL_SD_MEM_CURRENT_MONEY_BANK_INTEGER_LENGTH = 4,
            NOL_SD_MEM_CURRENT_MONEY_BANK_DECIMAL_LENGTH = 2,
            NOL_SD_MEM_CURRENT_MONEY_COMMON_BANK_LENGTH = 4
        }
    }
}
