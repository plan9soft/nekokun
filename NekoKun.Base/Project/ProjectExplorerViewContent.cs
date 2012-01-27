using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
    public class ProjectExplorerViewContent : AbstractViewContent
    {
        Project project;
        TreeView tree;

        public ProjectExplorerViewContent(Project project)
        {
            this.project = project;
            this.tree = project.ProjectExplorer.TreeView;
            this.TitleName = project.ProjectTitle;
            this.IsPad = true;
            this.IsViewOnly = false;
        }

        public override Control Control
        {
            get
            {
                return tree;
            }
        }
    }
}
