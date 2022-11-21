using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstRevitTool.Masterclass.Dockable
{
    public static class DialogUtils
    {
        public static string SelectFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xlsx",
                Multiselect = false
            };
            var result = dialog.ShowDialog();
            var filepath = dialog.FileName;

            return result != DialogResult.OK ? String.Empty : filepath;
        }

        public static string SelectFolder()
        {
            var folder = new FolderBrowserDialog();
            var result = folder.ShowDialog();
            var folderpath = folder.SelectedPath;
            return folderpath;
            
        }
    }
}
