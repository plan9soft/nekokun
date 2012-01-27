using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
    public abstract class AbstractProjectEngine : IProjectEngine
    {
        Project project;

        public AbstractProjectEngine(Project project)
        {
            this.project = project;
        }

        public Project Project
        {
            get
            {
                return project;
            }
        }
    }
}
