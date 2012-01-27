// Copyright (c) 2005 Daniel Grunwald
// Licensed under the terms of the "BSD License", see doc/license.txt

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
	public class TextDisplayBinding : IDisplayBinding
	{
		public IViewContent OpenFile(string fileName)
		{
			return new TextViewContent(fileName);
		}
	}
	
	/// <summary>
	/// ViewContent showing a text file.
	/// </summary>
    public class TextViewContent : AbstractViewContent, IUndoHandler, IClipboardHandler

	{
		TextBox textBox = new TextBox();
		
		public TextViewContent()
		{
            this.UntitledName = "UntitledText.txt";
            this.IsViewOnly = false;
			textBox.Multiline = true;
			textBox.AcceptsReturn = true;
			textBox.AcceptsTab = true;
			textBox.Font = new Font("Courier New", 10f);
			textBox.WordWrap = false;
			textBox.ScrollBars = ScrollBars.Both;
			textBox.TextChanged += delegate {
				this.IsDirty = true;
			};
            textBox.Dock = DockStyle.Fill;
            this.Icon = System.Drawing.Icon.FromHandle(ICSharpCode.Core.ResourceService.GetBitmap("Icons.Textbox").GetHicon());
		}
		
		public TextViewContent(string fileName) : this()
		{
            Load(fileName);
		}

        public override void Load(string fileName)
        {
            textBox.Text = File.ReadAllText(fileName);
            SetTitleAndFileName(fileName);
        }

        public override void Save(string fileName)
        {
            File.WriteAllText(fileName, textBox.Text);
            SetTitleAndFileName(fileName);
        }

        public override void Dispose()
        {
            textBox.Dispose();
            base.Dispose();
        }

		#region IClipboardHandler implementation
		bool IClipboardHandler.CanPaste {
			get {
				return !textBox.ReadOnly;
			}
		}
		
		bool IClipboardHandler.CanCut {
			get {
				return !textBox.ReadOnly && textBox.SelectionLength > 0;
			}
		}
		
		bool IClipboardHandler.CanCopy {
			get {
				return textBox.SelectionLength > 0;
			}
		}
		
		bool IClipboardHandler.CanDelete {
			get {
				return !textBox.ReadOnly && textBox.SelectionLength > 0;
			}
		}
		
		void IClipboardHandler.Paste()
		{
			textBox.Paste();
		}
		
		void IClipboardHandler.Cut()
		{
			textBox.Cut();
		}
		
		void IClipboardHandler.Copy()
		{
			textBox.Copy();
		}
		
		void IClipboardHandler.Delete()
		{
			textBox.SelectedText = "";
		}
		#endregion
		
		#region IUndoHandler implementation
		bool IUndoHandler.CanUndo {
			get {
				return textBox.CanUndo;
			}
		}
		
		bool IUndoHandler.CanRedo {
			get {
				return false;
			}
		}
		
		void IUndoHandler.Undo()
		{
			textBox.Undo();
		}
		
		void IUndoHandler.Redo()
		{
			throw new NotImplementedException();
		}
		#endregion

        public override Control Control
        {
            get { return textBox; }
        }
    }
}
