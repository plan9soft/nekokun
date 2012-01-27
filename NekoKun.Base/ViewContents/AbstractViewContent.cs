// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 2003 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;

namespace orzTech.NekoKun.Base
{
    public abstract class AbstractViewContent : AbstractBaseViewContent, IViewContent, ICanBeDirty
	{
		string untitledName = String.Empty;
		string titleName    = null;
		string fileName     = null;
        System.Drawing.Icon icon = null;
		bool   isViewOnly = false;

		public AbstractViewContent()
		{
		}
		
		public AbstractViewContent(string titleName)
		{
			this.titleName = titleName;
		}
		
		public AbstractViewContent(string titleName, string fileName)
		{
			this.titleName = titleName;
			this.fileName  = fileName;
		}

		/// <summary>
		/// Sets the file name to <paramref name="fileName"/> and the title to the file name without path. Sets dirty == false too.
		/// </summary>
		/// <param name="fileName">The name of the file currently inside the content.</param>
		protected void SetTitleAndFileName(string fileName)
		{
			TitleName = Path.GetFileName(fileName);
			FileName  = fileName;
			IsDirty   = false;
		}
		
		public event EventHandler FileNameChanged;
		
		protected virtual void OnFileNameChanged(EventArgs e)
		{
			if (FileNameChanged != null) {
				FileNameChanged(this, e);
			}
		}
		
		#region IViewContent implementation
		public virtual string UntitledName {
			get {
				return untitledName;
			}
			set {
				untitledName = value;
			}
		}
		
		public virtual string TitleName {
			get {
				return titleName ?? Path.GetFileName(untitledName);
			}
			set {
				titleName = value;
				OnTitleNameChanged(EventArgs.Empty);
			}
		}
		
		public virtual string FileName {
			get {
				return fileName;
			}
			set {
				fileName = value;
				OnFileNameChanged(EventArgs.Empty);
			}
		}
		
		public virtual bool IsUntitled {
			get {
				return titleName == null;
			}
		}
		
		public virtual bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public virtual bool IsViewOnly {
			get {
				return isViewOnly;
			}
			set {
				isViewOnly = value;
			}
		}
		

		public virtual void Save()
		{
			if (IsDirty) {
				Save(fileName);
			}
		}
		
		public virtual void Save(string fileName)
		{
			throw new System.NotImplementedException();
		}
		
		public virtual void Load(string fileName)
		{
			throw new System.NotImplementedException();
		}
		
		
		public event EventHandler TitleNameChanged;
        public event EventHandler IconChanged;
		public event EventHandler     Saving;
		public event SaveEventHandler Saved;

		protected virtual void OnTitleNameChanged(EventArgs e)
		{
			if (TitleNameChanged != null) {
				TitleNameChanged(this, e);
			}
		}

        protected virtual void OnIconChanged(EventArgs e)
        {
            if (IconChanged != null)
            {
                IconChanged(this, e);
            }
        }

        public System.Drawing.Icon Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                OnIconChanged(EventArgs.Empty);
            }
        }

		protected virtual void OnSaving(EventArgs e)
		{
			if (Saving != null) {
				Saving(this, e);
			}
		}
		
		protected virtual void OnSaved(SaveEventArgs e)
		{
			if (Saved != null) {
				Saved(this, e);
			}
		}

		#region IBaseViewContent implementation
		// handled in AbstractBaseViewContent

		#region IDisposable implementation
		public override void Dispose()
		{
			base.Dispose();
		}
		#endregion
		#endregion
		
		#region ICanBeDirty implementation
		public virtual bool IsDirty {
			get {
				return isDirty;
			}
			set {
				if (isDirty != value) {
					isDirty = value;
					OnDirtyChanged(EventArgs.Empty);
				}
			}
		}
		bool   isDirty  = false;
		
		public event EventHandler DirtyChanged;

		protected virtual void OnDirtyChanged(EventArgs e)
		{
			if (DirtyChanged != null) {
				DirtyChanged(this, e);
			}
		}
		#endregion
		#endregion

        public override System.Windows.Forms.Control Control
        {
            get { throw new NotImplementedException(); }
        }

        bool isPad = false;
        public bool IsPad
        {
            get
            {
                return isPad; 
            }
            set
            {
                isPad = value;
            }
        }
    }
}
