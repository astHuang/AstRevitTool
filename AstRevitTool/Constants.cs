﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstRevitTool
{
    internal static class Constants
    {
        internal static string urlMain => "https://www.arrowstreet.com/";
   
        internal static string emailSupport => "marketing@arrowstreet.com";

        internal const string LeasingParameter = "Leasing Classification";

        internal const string ProgrammingParameter = "Program Group";

        internal const string BOMA = "BOMA Space Classification";

        internal const string GenslerRoomKey = "Room Building Use (Key)";
        internal const string GenslerAreaKey = "Area Building Use (Key)";

        internal const string EXCLUSION = "BOMA Rentable Exclusion";
        //internal const string SCRIPT_FOLDER = "W:\\5X\\50113_Data_Science\\04_InternalTools\\02-ASTRevitTool";
        internal const string SCRIPT_FOLDER = "W:\\5X\\50113_Data_Science\\04_InternalTools\\02-ASTRevitTool";

        public static string[] ColourValues = new string[] {
        "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
        "800000", "008000", "000080", "808000", "800080", "008080", "808080",
        "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
        "400000", "004000", "000040", "404000", "400040", "004040", "404040",
        "200000", "002000", "000020", "202000", "200020", "002020", "202020",
        "600000", "006000", "000060", "606000", "600060", "006060", "606060",
        "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
        "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
        };
    }
}
