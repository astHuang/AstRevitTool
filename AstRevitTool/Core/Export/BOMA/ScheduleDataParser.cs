using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace AstRevitTool.Core.Export
{
    class ScheduleDataParser
    {
        /// <summary>
        /// Default schedule data file field delimiter.
        /// </summary>
        static char[] _tabs = new char[] { '\t' };

        /// <summary>
        /// Strip the quotes around text strings 
        /// in the schedule data file.
        /// </summary>
        static char[] _quotes = new char[] { '"' };

        string _name = null;

        /// <summary>
        /// Schedule name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Schedule columns and row data
        /// </summary>


        public ScheduleDataParser(string filename)
        {
            StreamReader stream = File.OpenText(filename);

            string line;
            string[] a;

            while (null != (line = stream.ReadLine()))
            {
                a = line
                  .Split(_tabs)
                  .Select<string, string>(s => s.Trim(_quotes))
                  .ToArray();

                // First line of text file contains 
                // schedule name

                if (null == _name)
                {
                    _name = a[0];
                    continue;
                }

                // Second line of text file contains 
                // schedule column names

                foreach (string column_name in a)
                {
                }

             }

            stream.Close();
                // Remaining lines define schedula data

        }
    }
}
