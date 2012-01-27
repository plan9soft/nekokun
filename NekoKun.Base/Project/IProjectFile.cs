using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.Base
{
    public interface IProjectFile
    {
        string FileName
        {
            get;
        }
        void Save();

        IViewContent ViewContent
        {
            get;
        }
    }
}
