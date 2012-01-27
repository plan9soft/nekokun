using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
    public class ProjectExplorer
    {
        private TreeView treeView;

        public ProjectExplorer(Project project)
        {
            treeView = new TreeView();
            treeView.FullRowSelect = true;
            treeView.ShowLines = false;
            treeView.ShowRootLines = false;
            treeView.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeView_NodeMouseDoubleClick);
        }

        void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenNode(e.Node);
        }

        internal TreeView TreeView
        {
            get { return treeView; }
        }

        public void AddItem(ProjectItem item)
        {
            TreeNode node = new TreeNode(item.Name);
            node.Tag = item;
            this.treeView.Nodes.Add(node);
        }

        private void OpenNode(TreeNode node)
        {
            try
            {
                if ((node.Tag as ProjectItem).ProjectFile.ViewContent != null)
                    Workbench.Instance.ShowView((node.Tag as ProjectItem).ProjectFile.ViewContent);
            }
            catch { }
        }
    }
}
