using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public static class AcronymHelper
    {
        public static Dictionary<int, string> LoadAcronyms()
        {
            Dictionary<int, string> acronymMap = new();
            acronymMap[1] = "TSB";
            acronymMap[2] = "TCB";
            acronymMap[3] = "SYS";
            acronymMap[4] = "LAG";
            acronymMap[5] = "IOU";
            acronymMap[6] = "GRN";
            acronymMap[7] = "THS";
            acronymMap[8] = "YAZ";
            acronymMap[9] = "TCT";
            acronymMap[10] = "CNC";
            acronymMap[11] = "MSG";
            acronymMap[12] = "TMI";
            acronymMap[13] = "TCP";
            acronymMap[14] = "IIL";
            acronymMap[15] = "FHG";
            acronymMap[16] = "SYM";
            acronymMap[17] = "LSC";
            acronymMap[18] = "PRN";
            acronymMap[19] = "EWM";
            acronymMap[20] = "TCM";
            acronymMap[21] = "ELT";
            acronymMap[22] = "HRG";
            acronymMap[23] = "TGP";
            acronymMap[24] = "MUN";
            acronymMap[25] = "WSU";
            acronymMap[26] = "IST";
            acronymMap[27] = "BAG";
            acronymMap[28] = "EVL";
            acronymMap[29] = "MCS";
            acronymMap[30] = "WLT";
            acronymMap[31] = "TCC";
            acronymMap[32] = "ASS";

            return acronymMap;
        }
    }
}
