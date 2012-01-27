// Copyright (c) 2005 Daniel Grunwald
// Licensed under the terms of the "BSD License", see doc/license.txt

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using System.Collections.Generic;

namespace orzTech.NekoKun.Base
{
	/// <summary>
	/// The main form of the application.
	/// </summary>
	public sealed class Workbench : Form
	{
		static Workbench instance;
		
		public static Workbench Instance {
			get {
				return instance;
			}
		}
		
		public static void InitializeWorkbench()
		{
			instance = new Workbench();
		}
		
		MenuStrip menu;
		ToolStrip toolbar;
        private WeifenLuo.WinFormsUI.DockPanel dockPanel;
		
		private Workbench()
		{
			// restore form location from last session
			FormLocationHelper.Apply(this, "StartupFormPosition", true);

            dockPanel = new WeifenLuo.WinFormsUI.DockPanel();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.DocumentStyle = WeifenLuo.WinFormsUI.DocumentStyles.DockingMdi;
            dockPanel.ShowDocumentIcon = true;

            this.Controls.Add(dockPanel);
            this.Text = StringParser.Parse("${res:NekoKun.Workbench.Title}");
            this.IsMdiContainer = true;
            this.Name = "Workbench";
			
			menu = new MenuStrip();
			MenuService.AddItemsToMenu(menu.Items, this, "/Workbench/MainMenu");
			
			toolbar = ToolbarService.CreateToolStrip(this, "/Workbench/Toolbar");
			
			this.Controls.Add(toolbar);
			this.Controls.Add(menu);
			
			// Use the Idle event to update the status of menu and toolbar items.
			Application.Idle += OnApplicationIdle;
		}
		
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				Application.Idle -= OnApplicationIdle;
			}
			base.Dispose(disposing);
		}
		
		void OnApplicationIdle(object sender, EventArgs e)
		{
			// Use the Idle event to update the status of menu and toolbar.
			// Depending on your application and the number of menu items with complex conditions,
			// you might want to update the status less frequently.
			UpdateMenuItemStatus();
		}
		
		/// <summary>Update Enabled/Visible state of items in the main menu based on conditions</summary>
		void UpdateMenuItemStatus()
		{
			foreach (ToolStripItem item in menu.Items) {
				if (item is IStatusUpdate)
					(item as IStatusUpdate).UpdateStatus();
			}
		}

        public AbstractViewContent ActiveViewContent
        {
			get {
                try
                {
                    return (dockPanel.ActiveDocument as WorkspaceWindow).ViewContent as AbstractViewContent;
                }
                catch (Exception)
                {
                    return null;
                }
			}
		}
		
		protected override void OnClosing(CancelEventArgs e)
		{
            CloseAllViews();
            e.Cancel = closeAll;
            base.OnClosing(e);
		}

        public IViewContent ActiveContent
        {
            get
            {
                if (dockPanel == null)
                {
                    return null;
                }

                IWorkbenchWindow window = dockPanel.ActiveDocument as IWorkbenchWindow;
                if (window == null || window.IsDisposed)
                {
                    return null;
                }
                return window.ViewContent;
            }
        }

        List<IViewContent> workbenchContentCollection = new List<IViewContent>();
        public void CloseContent(IViewContent content)
        {
            while (workbenchContentCollection.Contains(content))
            {
                workbenchContentCollection.Remove(content);
            }
            //OnViewClosed(new ViewContentEventArgs(content));
            content.Dispose();
            content = null;
        }

        public List<IViewContent> ViewContentCollection
        {
            get
            {
                System.Diagnostics.Debug.Assert(workbenchContentCollection != null);
                return workbenchContentCollection;
            }
        }

        bool closeAll = false;
        public void CloseAllViews()
        {
            try
            {
                closeAll = true;
                List<IViewContent> fullList = new List<IViewContent>(workbenchContentCollection);
                foreach (IViewContent content in fullList)
                {
                    IWorkbenchWindow window = content.WorkbenchWindow;
                    window.CloseWindow(false);
                }
            }
            finally
            {
                closeAll = false;
                //OnActiveWindowChanged(this, EventArgs.Empty);
            }
        }

        public void ShowView(IViewContent content)
        {
            workbenchContentCollection.Add(content);

            LayoutShowView(content);
            content.WorkbenchWindow.SelectWindow();
            //OnViewOpened(new ViewContentEventArgs(content));
        }

        private IWorkbenchWindow LayoutShowView(IViewContent content)
        {
            if (content.WorkbenchWindow is WorkspaceWindow)
            {
                WorkspaceWindow oldSdiWindow = (WorkspaceWindow)content.WorkbenchWindow;
                if (!oldSdiWindow.IsDisposed)
                {
                    oldSdiWindow.Show(dockPanel);
                    return oldSdiWindow;
                }
            }
            if (!content.Control.Visible)
            {
                content.Control.Visible = true;
            }
            content.Control.Dock = DockStyle.Fill;
            WorkspaceWindow workspaceWindow = new WorkspaceWindow(content);
            workspaceWindow.CloseEvent += new EventHandler(CloseWindowEvent);
            if (dockPanel != null)
            {
                workspaceWindow.Show(dockPanel);
            }

            return workspaceWindow;
        }

        public void CloseWindowEvent(object sender, EventArgs e)
        {
            WorkspaceWindow f = (WorkspaceWindow)sender;
            f.CloseEvent -= CloseWindowEvent;
            if (f.ViewContent != null)
            {
                this.CloseContent(f.ViewContent);
            }
        }
    }
}
