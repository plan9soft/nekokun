// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 2015 $</version>
// </file>

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.Core;
using WeifenLuo.WinFormsUI;

namespace orzTech.NekoKun.Base
{
	public class WorkspaceWindow : DockContent, IWorkbenchWindow, IOwnerState
	{
		readonly static string contextMenuPath = "/Workbench/OpenFileTab/ContextMenu";
		
		#region IOwnerState
		[Flags]
		public enum OpenFileTabState {
			Nothing             = 0,
			FileDirty           = 1,
			FileReadOnly        = 2,
			FileUntitled        = 4
		}
		
		public System.Enum InternalState {
			get {
				OpenFileTabState state = OpenFileTabState.Nothing;
				if (content != null) {
					if (content.IsDirty)    state |= OpenFileTabState.FileDirty;
					if (content.IsReadOnly) state |= OpenFileTabState.FileReadOnly;
					if (content.IsUntitled) state |= OpenFileTabState.FileUntitled;
				}
				return state;
			}
		}
		#endregion

		IViewContent content;
		
		public string Title {
			get {
				return Text;
			}
			set {
				Text = value;
				OnTitleChanged(EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// The current view content which is shown inside this window.
		/// This method is thread-safe.
		/// </summary>
		public IBaseViewContent ActiveViewContent {
			get {
				return content;
			}
		}
				
		public void SelectWindow()
		{
			Show();
		}
		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		
		public WorkspaceWindow(IViewContent content)
		{
			this.content = content;
			content.WorkbenchWindow = this;
			
			content.TitleNameChanged += new EventHandler(SetTitleEvent);
            content.IconChanged      += new EventHandler(SetIconEvent);
			content.DirtyChanged     += new EventHandler(SetTitleEvent);

            if (content.IsPad)
                this.DockableAreas = DockAreas.DockBottom | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.Float;
            else
                this.DockableAreas = DockAreas.Document;

			this.DockPadding.All = 2;

			SetTitleEvent(this, EventArgs.Empty);
            SetIconEvent(this, EventArgs.Empty);
			this.TabPageContextMenuStrip = MenuService.CreateContextMenu(this, contextMenuPath);
			InitControls();
			
			//ParserService.LoadSolutionProjectsThreadEnded += LoadSolutionProjectsThreadEndedEvent;
		}

        void SetIconEvent(object sender, EventArgs e)
        {
            try
            {
                this.Icon = this.content.Icon;
            }
            catch (Exception)
            {
                return;
            }
        }
	
		internal void InitControls()
		{
			content.Control.Dock = DockStyle.Fill;
			Controls.Add(content.Control);
		}
		
		public IViewContent ViewContent {
			get {
				return content;
			}
		}
		
		void SetToolTipText()
		{
			if (content != null) {
				try {
					if (content.FileName != null && content.FileName.Length > 0) {
						base.ToolTipText = Path.GetFullPath(content.FileName);
					} else {
						base.ToolTipText = null;
					}
				} catch (Exception) {
					base.ToolTipText = content.FileName;
				}
			} else {
				base.ToolTipText = null;
			}
		}
		
		public void SetTitleEvent(object sender, EventArgs e)
		{
			if (content == null) {
				return;
			}
			SetToolTipText();
			string newTitle = content.TitleName;
			
			if (content.IsDirty) {
				newTitle += "*";
			} else if (content.IsReadOnly) {
				newTitle += "+";
			}
			
			if (newTitle != Title) {
				Title = newTitle;
			}
		}
		
		
		public bool CloseWindow(bool force)
		{
			if (!force && ViewContent != null && ViewContent.IsDirty) {
				DialogResult dr = MessageBox.Show(ResourceService.GetString("MainWindow.SaveChangesMessage"),
				                                  ResourceService.GetString("MainWindow.SaveChangesMessageHeader") + " " + Title + " ?",
				                                  MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
				                                  MessageBoxDefaultButton.Button1,
				                                  RightToLeftConverter.IsRightToLeft ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
				switch (dr) {
					case DialogResult.Yes:
						if (content.FileName == null) {
							while (true) {
                                ICommand saveAs = new NekoKun.Base.Commands.SaveFileAsCommand();
                                saveAs.Owner = Workbench.Instance;
								saveAs.Run();
								if (ViewContent.IsDirty) {
									
									if (MessageService.AskQuestion("${res:MainWindow.DiscardChangesMessage}")) {
										break;
									}
								} else {
									break;
								}
							}
							
						} else {
							
							FileUtility.ObservedSave(new FileOperationDelegate(ViewContent.Save), ViewContent.FileName , FileErrorPolicy.ProvideAlternative);
						}
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						return false;
				}
			}

			OnCloseEvent(null);
			Dispose();
			return true;
		}
		
		
		protected virtual void OnTitleChanged(EventArgs e)
		{
			if (TitleChanged != null) {
				TitleChanged(this, e);
			}
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !CloseWindow(false);
		}
		
		protected virtual void OnCloseEvent(EventArgs e)
		{
			OnWindowDeselected(e);
			if (CloseEvent != null) {
				CloseEvent(this, e);
			}
		}
		
		public virtual void OnWindowSelected(EventArgs e)
		{
			if (WindowSelected != null) {
				WindowSelected(this, e);
			}
		}
		public virtual void OnWindowDeselected(EventArgs e)
		{
			if (WindowDeselected != null) {
				WindowDeselected(this, e);
			}
		}
		
		public event EventHandler WindowSelected;
		public event EventHandler WindowDeselected;
		
		public event EventHandler TitleChanged;
		public event EventHandler CloseEvent;
	}
}
