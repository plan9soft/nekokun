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
            List<object> editorScripts = RubyMarshal.Load(file, true) as List<object>;


            foreach (List<object> item in editorScripts)
            {
                string title;
                byte[] bytes;
                if (item[1] is RubyExpendObject)
                    title = UnicodeStringFromUTF8Bytes((byte[])((RubyExpendObject)item[1]).BaseObject);
                else
                    title = UnicodeStringFromUTF8Bytes((byte[])item[1]);

                if (item[2] is RubyExpendObject)
                    bytes = (byte[])((RubyExpendObject)item[2]).BaseObject;
                else
                    bytes = (byte[])item[2];

                byte[] inflated = Ionic.Zlib.ZlibStream.UncompressBuffer(bytes);
                string code = "";
                if (inflated.Length > 0) code = System.Text.Encoding.UTF8.GetString(inflated);

                scripts.Add(new RGSSScriptItem(scripts, title, code));
            }

            return scripts;
        }

        private static string UnicodeStringFromUTF8Bytes(byte[] bytes)
        {
            return Encoding.Unicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes));
        }

        private static byte[] UTF8BytesFromUnicodeString(string str)
        {
            return Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(str));
        }

        public string FileName
        {
            get { return this.fileName; }
            protected set { this.fileName = value; }
        }

        public void Save()
        {
            List<object> rawFile = new List<object>();
            Random random = new Random();
            foreach (RGSSScriptItem item in this)
            {
                if (item.HasViewContentLoaded)
                    item.ViewContent.SubmitChange();
            }

            foreach (RGSSScriptItem item in this)
            {
                List<object> rawItem = new List<object>();
                rawItem.Add(random.Next(1000));
                rawItem.Add(UTF8BytesFromUnicodeString(item.Title));
                rawItem.Add(Ionic.Zlib.ZlibStream.CompressBuffer(UTF8BytesFromUnicodeString(item.Code)));
                rawFile.Add(rawItem);
            }
            System.IO.FileStream file = System.IO.File.OpenWrite(this.fileName);
            RubyMarshal.Dump(file, rawFile);
            file.Close();

            foreach (RGSSScriptItem item in this)
            {
                if (item.HasViewContentLoaded)
                    item.ViewContent.IsDirty = false;
            }
        }

        public IViewContent ViewContent
        {
            get {
                return null;
            }
        }
    }
}
