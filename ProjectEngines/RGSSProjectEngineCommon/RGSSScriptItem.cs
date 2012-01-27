using System;
using System.Collections.Generic;
using System.Text;
using orzTech.NekoKun.Base;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RGSSScriptItem : IProjectFile
    {
        public string Title;
        public RGSSScriptFile ScriptFile;
        private IViewContent viewContent;
        public string Code;

        public RGSSScriptItem(RGSSScriptFile file) 
            : this(file, "", "") { }
        public RGSSScriptItem(RGSSScriptFile file, string title) 
            : this(file, title, "") { }
        public RGSSScriptItem(RGSSScriptFile file, string title, string code)
        {
            this.Title = title;
            this.Code = code;
            this.ScriptFile = file;
        }

        public string FileName
        {
            get { return this.ScriptFile.FileName; }
        }

        public void Save()
        {
            this.ScriptFile.Save();
        }

        public IViewContent ViewContent
        {
            get
            {
                if (viewContent != null && !viewContent.IsDisposed) return viewContent;
                return viewContent = new RGSSScriptItemViewContent(this);
            }
        }
    }
}
