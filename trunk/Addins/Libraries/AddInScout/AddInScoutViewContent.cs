// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using orzTech.NekoKun.Base;

namespace AddInScout
{
    public class AddInScoutViewContent : AbstractViewContent
    {
        Control control = null;

        public override Control Control
        {
            get
            {
                return control;
            }
        }

        public override bool IsViewOnly
        {
            get
            {
                return true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            control.Dispose();
        }

        public override void Load(string filename)
        {
        }

        AddInDetailsPanel addInDetailsPanel = new AddInDetailsPanel();
        CodonListPanel codonListPanel = new CodonListPanel();
        Splitter hs = new Splitter();
        TabControl tab;

        public AddInScoutViewContent()
            : base("AddIn Scout")
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.BorderStyle = BorderStyle.FixedSingle;

            Panel RightPanel = new Panel();
            RightPanel.Dock = DockStyle.Fill;
            p.Controls.Add(RightPanel);

            codonListPanel.Dock = DockStyle.Fill;
            codonListPanel.CurrentAddinChanged += new EventHandler(CodonListPanelCurrentAddinChanged);
            RightPanel.Controls.Add(codonListPanel);

            hs.Dock = DockStyle.Top;
            RightPanel.Controls.Add(hs);

            addInDetailsPanel.Dock = DockStyle.Top;
            addInDetailsPanel.Height = 175;
            RightPanel.Controls.Add(addInDetailsPanel);

            Splitter s1 = new Splitter();
            s1.Dock = DockStyle.Left;
            p.Controls.Add(s1);

            AddinTreeView addinTreeView = new AddinTreeView();
            addinTreeView.Dock = DockStyle.Fill;
            addinTreeView.treeView.AfterSelect += new TreeViewEventHandler(this.tvSelectHandler);

            TreeTreeView treeTreeView = new TreeTreeView();
            treeTreeView.Dock = DockStyle.Fill;
            treeTreeView.treeView.AfterSelect += new TreeViewEventHandler(this.tvSelectHandler);

            tab = new TabControl();
            tab.Width = 300;
            tab.Dock = DockStyle.Left;
            tab.SelectedIndexChanged += new EventHandler(this.SelectedIndexChanged);

            TabPage tabPage2 = new TabPage("Tree");
            tabPage2.Dock = DockStyle.Left;
            tabPage2.Controls.Add(treeTreeView);
            tab.TabPages.Add(tabPage2);

            TabPage tabPage = new TabPage("AddIns");
            tabPage.Dock = DockStyle.Left;
            tabPage.Controls.Add(addinTreeView);
            tab.TabPages.Add(tabPage);

            p.Controls.Add(tab);
            SelectedIndexChanged(tab, EventArgs.Empty);

            this.Icon = System.Drawing.Icon.FromHandle(ResourceService.GetBitmap("Icons.Assembly").GetHicon());
            this.control = p;
            this.TitleName = "AddIn Scout";
        }

        void SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tab.SelectedIndex == 1)
            {
                hs.Dock = DockStyle.Top;
                addInDetailsPanel.Dock = DockStyle.Top;
            }
            else
            {
                hs.Dock = DockStyle.Bottom;
                addInDetailsPanel.Dock = DockStyle.Bottom;
            }
        }

        void CodonListPanelCurrentAddinChanged(object sender, EventArgs e)
        {
            addInDetailsPanel.ShowAddInDetails(codonListPanel.CurrentAddIn);
        }

        public void tvSelectHandler(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                codonListPanel.ClearList();
                return;
            }

            TreeNode tn = e.Node;

            object o = e.Node.Tag;

            if (o is AddIn)
            {
                AddIn addIn = (AddIn)o;
                addInDetailsPanel.ShowAddInDetails(addIn);
                if (tn.FirstNode != null)
                {
                    codonListPanel.ListCodons((ExtensionPath)tn.FirstNode.Tag);
                }
                else
                {
                    codonListPanel.ClearList();
                }
            }
            else
            {
                ExtensionPath ext = (ExtensionPath)o;
                AddIn addIn = tn.Parent.Tag as AddIn;
                if (addIn == null)
                {
                    codonListPanel.ListCodons(ext.Name);
                }
                else
                {
                    addInDetailsPanel.ShowAddInDetails(addIn);
                    codonListPanel.ListCodons(ext);
                }
            }
        }
    }
}
