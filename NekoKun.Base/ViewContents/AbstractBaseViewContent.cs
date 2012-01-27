// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1185 $</version>
// </file>

using System;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
	public abstract class AbstractBaseViewContent : IBaseViewContent
	{
        bool isDisposed = false;

		IWorkbenchWindow workbenchWindow = null;
		
		public event EventHandler WorkbenchWindowChanged;
		
		protected virtual void OnWorkbenchWindowChanged(EventArgs e)
		{
			if (WorkbenchWindowChanged != null) {
				WorkbenchWindowChanged(this, e);
			}
		}
		
		#region IBaseViewContent implementation
		public abstract Control Control {
			get;
		}
		
		public virtual IWorkbenchWindow WorkbenchWindow {
			get {
				return workbenchWindow;
			}
			set {
				workbenchWindow = value;
				OnWorkbenchWindowChanged(EventArgs.Empty);
			}
		}
		
		public virtual string TabPageText {
			get {
				return "Abstract Content";
			}
		}
		
		public virtual void SwitchedTo()
		{
		}
		
		public virtual void Selected()
		{
		}
		
		public virtual void Deselected()
		{
		}

		public virtual void Deselecting()
		{
		}
		
		public virtual void RedrawContent()
		{
		}
		
		#region IDisposable implementation
		public virtual void Dispose()
		{
			workbenchWindow = null;
            isDisposed = true;
		}
		#endregion
		
		#endregion		
	
        #region IBaseViewContent 成员


        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        #endregion
    }
}
