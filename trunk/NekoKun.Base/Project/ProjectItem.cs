using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.Base
{
    public class ProjectItem
    {
        public string Name;

        public IProjectFile ProjectFile;

        public ProjectItem(string Name, IProjectFile ProjectFile)
        {
            this.Name = Name;
            this.ProjectFile = ProjectFile;
        }
    }
}
