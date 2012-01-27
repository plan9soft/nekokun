// Copyright (c) 2005 Daniel Grunwald
// Licensed under the terms of the "BSD License", see doc/license.txt

using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using System.IO;

namespace orzTech.NekoKun.Base.Commands
{
	public class NewFileCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			Workbench workbench = (Workbench)this.Owner;
			workbench.ShowView(new TextViewContent());
		}
	}
	
	public class OpenFileCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			Workbench workbench = (Workbench)this.Owner;
			using (OpenFileDialog dlg = new OpenFileDialog()) {
				dlg.CheckFileExists = true;
				dlg.DefaultExt = ".txt";
				dlg.Filter = FileService.GetFileFilter("/Workspace/FileFilter");
				if (dlg.ShowDialog() == DialogResult.OK) {
					IViewContent content = DisplayBindingManager.CreateViewContent(dlg.FileName);
					if (content != null) {
						workbench.ShowView(content as orzTech.NekoKun.Base.AbstractViewContent);
					}
				}
			}
		}
	}

    public class OpenProjectCommand : AbstractMenuCommand
    {
        public override void Run()
        {
            Workbench workbench = (Workbench)this.Owner;
            if (ProjectService.ActiveProject != null)
            {
                if (MessageService.AskQuestion("${res:ProjectService.ProjectAlreadyOpened}"))
                {
                    if (!ProjectService.CloseActiveProject())
                        return;
                }
                else { return; }
            }
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckFileExists = true;
                dlg.DefaultExt = ".txt";
                dlg.Filter = ProjectService.ProjectFileFilter;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ProjectService.OpenProject(dlg.FileName);
                }
            }
        }
    }
	
	public class SaveFileCommand : AbstractMenuCommand
	{
		public override void Run()
		{
            AbstractViewContent avc = null;
			if (this.Owner is Workbench) {
                Workbench workbench = (Workbench)this.Owner;
                avc = workbench.ActiveViewContent;
            }
            else if (this.Owner is WorkspaceWindow)
            {
                avc = (this.Owner as WorkspaceWindow).ViewContent as AbstractViewContent;
            }
            if (avc == null)
                return;

            if (avc.IsViewOnly) {
				return;
			}
            if (avc.FileName == null)
            {
                ICommand sfa = new SaveFileAsCommand();
                sfa.Owner = avc.WorkbenchWindow;
                sfa.Run();
            }
            else
            {
                FileAttributes attr = FileAttributes.ReadOnly | FileAttributes.Directory | FileAttributes.Offline | FileAttributes.System;
				if (File.Exists(avc.FileName) && (File.GetAttributes(avc.FileName) & attr) != 0) {
                    ICommand sfa = new SaveFileAsCommand();
                    sfa.Owner = avc.WorkbenchWindow;
                    sfa.Run();
				} else {
					FileUtility.ObservedSave(new NamedFileOperationDelegate(avc.Save), avc.FileName);
				}
            }
		}
	}
	
	public class SaveFileAsCommand : AbstractMenuCommand
	{
		public override void Run()
		{
            AbstractViewContent avc = null;
            if (this.Owner is Workbench)
            {
                Workbench workbench = (Workbench)this.Owner;
                avc = workbench.ActiveViewContent;
            }
            else if (this.Owner is WorkspaceWindow)
            {
                avc = (this.Owner as WorkspaceWindow).ViewContent as AbstractViewContent;
            }
            if (avc == null)
                return;

            using (SaveFileDialog fdiag = new SaveFileDialog())
            {
                fdiag.OverwritePrompt = true;
                fdiag.AddExtension = true;

                string[] fileFilters = (string[])(AddInTree.GetTreeNode("/Workspace/FileFilter").BuildChildItems(this)).ToArray(typeof(string));
                fdiag.Filter = String.Join("|", fileFilters);
                for (int i = 0; i < fileFilters.Length; ++i)
                {
                    if (fileFilters[i].IndexOf(Path.GetExtension(avc.FileName == null ? avc.UntitledName : avc.FileName)) >= 0)
                    {
                        fdiag.FilterIndex = i + 1;
                        break;
                    }
                }

                if (fdiag.ShowDialog(Workbench.Instance) == DialogResult.OK)
                {
                    string fileName = fdiag.FileName;
                    if (!FileService.CheckFileName(fileName))
                    {
                        return;
                    }
                    if (FileUtility.ObservedSave(new NamedFileOperationDelegate(avc.Save), fileName) == FileOperationResult.OK)
                    {
                        //FileService.RecentOpen.AddLastFile(fileName);
                    }
                }
            }
		}
	}
	
	public class ExitCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			Workbench workbench = (Workbench)this.Owner;
            workbench.Close();
		}
	}
}
