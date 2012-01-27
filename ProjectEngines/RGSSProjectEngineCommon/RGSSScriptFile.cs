using System;
using System.Collections.Generic;
using System.Text;
using orzTech.NekoKun.Base;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RGSSScriptFile : List<RGSSScriptItem>, IProjectFile
    {
        private string fileName;

        public RGSSScriptFile() : base()
        {
            
        }

        public static RGSSScriptFile LoadFile(string fileName)
        { 
            RGSSScriptFile scripts;
            using (System.IO.FileStream scriptFile = System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                scripts = LoadFile(scriptFile);
                scriptFile.Close();
            }
            scripts.FileName = fileName;
            return scripts;
        }

        protected static RGSSScriptFile LoadFile(System.IO.FileStream file)
        {
            RGSSScriptFile scripts = new RGSSScriptFile();
            List<object> editorScripts = Marshal.Load(file, true) as List<object>;


            foreach (List<object> item in editorScripts)
            {
                string title;
                byte[] bytes;
                if (item[1] is RubyExpendObject)
                    title = StringFromBytesUTF8toUnicode((byte[])((RubyExpendObject)item[1]).BaseObject);
                else
                    title = StringFromBytesUTF8toUnicode((byte[])item[1]);

                if (item[2] is RubyExpendObject)
                    bytes = (byte[])((RubyExpendObject)item[2]).BaseObject;
                else
                    bytes = (byte[])item[2];

                IronRuby.Builtins.MutableString ms = IronRuby.Builtins.MutableString.CreateBinary(bytes);
                IronRuby.Builtins.MutableString inflated = IronRuby.StandardLibrary.Zlib.Zlib.Inflate.InflateString(new IronRuby.StandardLibrary.Zlib.Zlib.Inflate(), ms);
                string code = "";
                if (inflated.Length > 0) code = System.Text.Encoding.UTF8.GetString(inflated.ToByteArray());

                scripts.Add(new RGSSScriptItem(scripts, title, code));
            }

            return scripts;
        }

        private static string StringFromBytesUTF8toUnicode(byte[] bytes)
        {
            byte[] buffer = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
            return Encoding.Unicode.GetString(buffer);
        }

        public string FileName
        {
            get { return this.fileName; }
            protected set { this.fileName = value; }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public IViewContent ViewContent
        {
            get {
                return null;
            }
        }
    }
}
