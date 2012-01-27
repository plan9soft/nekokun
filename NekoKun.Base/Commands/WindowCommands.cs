using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using System.IO;

namespace orzTech.NekoKun.Base.Commands
{
    public class CloseWindowCommand : AbstractMenuCommand
    {
        public override void Run()
        {
            if (this.Owner is WorkspaceWindow)
            {
                WorkspaceWindow view = this.Owner as WorkspaceWindow;
                view.CloseWindow(false);
            }
            else if (this.Owner is Workbench)
            {
                try
                {
                    Workbench.Instance.ActiveContent.WorkbenchWindow.CloseWindow(false);
                }
                catch (Exception) { }
            }
        }
    }

    public class OpenWindowsMenuBuilder : ISubmenuBuilder
    {

        class MyMenuItem : MenuCheckBox
        {
            IViewContent content;
            public MyMenuItem(IViewContent content)
                : base(StringParser.Parse(content.TitleName))
            {
                this.content = content;
                this.CheckedChanged += new EventHandler(MyMenuItem_CheckedChanged);
                MyMenuItem_CheckedChanged(this, EventArgs.Empty);
            }

            void MyMenuItem_CheckedChanged(object sender, EventArgs e)
            {
                if (!this.Checked)
                {
                    if (this.content.Icon != null)
                        this.Image = this.content.Icon.ToBitmap();
                }
                else
                    this.Image = null;
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                Checked = true;
                content.WorkbenchWindow.SelectWindow();
            }
        }

        public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
        {
            if (Workbench.Instance == null) return new ToolStripItem[] { };
            int contentCount = Workbench.Instance.ViewContentCollection.Count;
            if (contentCount == 0)
            {
                return new ToolStripItem[] { };
            }
            System.Collections.Generic.List<ToolStripItem> items = new System.Collections.Generic.List<ToolStripItem>();
            items.Add(new MenuSeparator(null, null));
            for (int i = 0; i < contentCount; ++i)
            {
                IViewContent content = (IViewContent)Workbench.Instance.ViewContentCollection[i];
                if (content.WorkbenchWindow == null)
                {
                    continue;
                }
                if (content.IsPad)
                    continue;
                MenuCheckBox item = new MyMenuItem(content);
                item.Tag = content.WorkbenchWindow;
                item.Checked = Workbench.Instance.ActiveContent.WorkbenchWindow == content.WorkbenchWindow;
                items.Add(item);
            }
            return items.ToArray();
        }
    }
}
