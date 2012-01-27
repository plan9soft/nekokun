using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using System.Runtime.InteropServices;

namespace orzTech.NekoKun.Base
{
    public class FileService
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key,string val,string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key,string def, StringBuilder retVal,
            int size,string filePath);

        public static void IniWriteValue(string path, string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        public static string IniReadValue(string path, string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            255, path);
            return temp.ToString();
        }

        public static bool CheckFileName(string fileName)
        {
            if (FileUtility.IsValidFileName(fileName))
                return true;
            MessageService.ShowMessage(StringParser.Parse("${res:NekoKun.Commands.SaveFile.InvalidFileNameError}", new string[,] { { "FileName", fileName } }));
            return false;
        }

        /// <summary>
        /// Checks that a single directory name is valid.
        /// </summary>
        /// <param name="name">A single directory name not the full path</param>
        public static bool CheckDirectoryName(string name)
        {
            if (FileUtility.IsValidDirectoryName(name))
                return true;
            MessageService.ShowMessage(StringParser.Parse("${res:NekoKun.Commands.SaveFile.InvalidFileNameError}", new string[,] { { "FileName", name } }));
            return false;
        }

        public static string GetFileFilter(string addInTreePath)
        {
            StringBuilder b = new StringBuilder();
            b.Append("${res:Global.FileDialogAllKnownFileTypes}|");
            foreach (string filter in AddInTree.BuildItems(addInTreePath, null, true))
            {
                b.Append(filter.Substring(filter.IndexOf('|') + 1));
                b.Append(';');
            }
            foreach (string filter in AddInTree.BuildItems(addInTreePath, null, true))
            {
                b.Append('|');
                b.Append(filter);
            }
            b.Append("|${res:Global.FileDialogAllFileTypes}|*.*");
            return StringParser.Parse(b.ToString());
        }
    }
}
