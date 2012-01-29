using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using orzTech.NekoKun.Base;
using System.Collections.Generic;
using ScintillaNet;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RGSSScriptItemViewContent : AbstractViewContent, IClipboardHandler, IUndoHandler
    {
        RGSSScriptItem scriptItem;
        Scintilla editor;

        public RGSSScriptItemViewContent(RGSSScriptItem scriptItem)
        {
            this.scriptItem = scriptItem;
            this.IsViewOnly = false;            
            this.TitleName = this.scriptItem.Title;
            this.FileName = this.scriptItem.ScriptFile.FileName;
            this.IsDirty = false;

            editor = new Scintilla();
            editor.Dock = DockStyle.Fill;
            editor.Font = new Font("SimSun_YaHei", 12);

            // http://ondineyuga.com/svn/RGE2/Tools/RGESEditor/RGESEditor_lang/EditorScintilla/Scintilla.cs
            // line number
            editor.Margins[0].Width = 39;

            // fold
            editor.Margins[1].Type = MarginType.Symbol;
            editor.Margins[1].Mask = -33554432; //SC_MASK_FOLDERS
            editor.Margins[1].Width = 16;
            editor.Margins[1].IsClickable = true;
            editor.NativeInterface.SetProperty("fold", "1");
            editor.NativeInterface.SetProperty("fold.comment", "0");
            editor.NativeInterface.SetProperty("fold.compact", "1");
            editor.Folding.Flags = FoldFlag.LineAfterContracted;

            // lexing
            editor.Lexing.Lexer = Lexer.Ruby;
            editor.Lexing.SetKeywords(0, "__FILE__ __LINE__ BEGIN END alias and begin break case class def defined? do else elsif end ensure false for if in module next nil not or redo rescue retry return self super then true undef unless until when while yield");
            editor.Styles.ClearAll();
            editor.UseFont = true;
            editor.Styles[(int)SCE_RB.DEFAULT].ForeColor = Color.FromArgb(0, 0, 0);
            editor.Styles[(int)SCE_RB.DEFAULT].BackColor = Color.FromArgb(255, 255, 255);
            editor.Styles[(int)SCE_RB.WORD].ForeColor = Color.FromArgb(0, 0, 127);
            editor.Styles[(int)SCE_RB.WORD_DEMOTED].ForeColor = Color.FromArgb(0, 0, 127);
            editor.Styles[(int)SCE_RB.STRING].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.GLOBAL].ForeColor = Color.FromArgb(180, 0, 180);
            editor.Styles[(int)SCE_RB.CLASSNAME].ForeColor = Color.FromArgb(0, 0, 255);
            editor.Styles[(int)SCE_RB.MODULE_NAME].ForeColor = Color.FromArgb(160, 0, 160);
            editor.Styles[(int)SCE_RB.CLASS_VAR].ForeColor = Color.FromArgb(128, 0, 204);
            editor.Styles[(int)SCE_RB.INSTANCE_VAR].ForeColor = Color.FromArgb(176, 0, 128);
            editor.Styles[(int)SCE_RB.NUMBER].ForeColor = Color.FromArgb(0, 127, 127);
            editor.Styles[(int)SCE_RB.STRING_Q].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.STRING_QQ].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.STRING_QX].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.STRING_QR].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.STRING_QW].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.REGEX].ForeColor = Color.FromArgb(120, 0, 170);
            editor.Styles[(int)SCE_RB.SYMBOL].ForeColor = Color.FromArgb(205, 100, 30);
            editor.Styles[(int)SCE_RB.DEFNAME].ForeColor = Color.FromArgb(0, 127, 127);
            editor.Styles[(int)SCE_RB.BACKTICKS].ForeColor = Color.FromArgb(160, 65, 10);
            editor.Styles[(int)SCE_RB.HERE_DELIM].ForeColor = Color.FromArgb(0, 137, 0);
            editor.Styles[(int)SCE_RB.HERE_Q].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.HERE_QQ].ForeColor = Color.FromArgb(127, 0, 151);
            editor.Styles[(int)SCE_RB.HERE_QX].ForeColor = Color.FromArgb(0, 137, 0);
            editor.Styles[(int)SCE_RB.DATASECTION].ForeColor = Color.FromArgb(127, 0, 0);
            editor.Styles[(int)SCE_RB.COMMENTLINE].ForeColor = Color.FromArgb(0, 127, 0);
            editor.Styles[(int)SCE_RB.POD].ForeColor = Color.FromArgb(0, 127, 0);
            
            editor.Text = this.scriptItem.Code;
            editor.UndoRedo.EmptyUndoBuffer();
            editor.TextDeleted += new EventHandler<TextModifiedEventArgs>(editor_TextDeleted);
            editor.TextInserted += new EventHandler<TextModifiedEventArgs>(editor_TextInserted);
            editor.Scrolling.HorizontalWidth = 1;
        }

        void editor_TextInserted(object sender, TextModifiedEventArgs e)
        {
            this.IsDirty = true;
        }

        void editor_TextDeleted(object sender, TextModifiedEventArgs e)
        {
            this.IsDirty = true;
        }

        private int ScintillaRGB(int R, int G, int B)
        {
            return R + (G << 8) + (B << 16);
        }

        public override Control Control
        {
            get { return editor; }
        }

        public override void Save(string fileName)
        {
            SubmitChange();
            this.scriptItem.ScriptFile.Save();
        }

        public override void SubmitChange()
        {
            this.scriptItem.Code = editor.Text;
        }

        public override void Dispose()
        {
            editor.Dispose();
            base.Dispose();
        }

        #region IClipboardHandler implementation
        bool IClipboardHandler.CanPaste
        {
            get
            {
                return editor.Clipboard.CanPaste;
            }
        }

        bool IClipboardHandler.CanCut
        {
            get
            {
                return editor.Selection.Length > 0;
            }
        }

        bool IClipboardHandler.CanCopy
        {
            get
            {
                return editor.Selection.Length > 0;
            }
        }

        bool IClipboardHandler.CanDelete
        {
            get
            {
                return editor.Selection.Length > 0;
            }
        }

        void IClipboardHandler.Paste()
        {
            editor.Clipboard.Paste();
        }

        void IClipboardHandler.Cut()
        {
            editor.Clipboard.Cut();
        }

        void IClipboardHandler.Copy()
        {
            editor.Clipboard.Copy();
        }

        void IClipboardHandler.Delete()
        {
            editor.Selection.Text = "";
        }
        #endregion

        #region IUndoHandler implementation
        bool IUndoHandler.CanUndo
        {
            get
            {
                return editor.UndoRedo.CanUndo;
            }
        }

        bool IUndoHandler.CanRedo
        {
            get
            {
                return editor.UndoRedo.CanRedo;
            }
        }

        void IUndoHandler.Undo()
        {
            editor.UndoRedo.Undo();
        }

        void IUndoHandler.Redo()
        {
            editor.UndoRedo.Redo();
        }
        #endregion

        private enum SCE_RB
        {
            DEFAULT = 0,
            ERROR = 1,
            COMMENTLINE = 2,
            POD = 3,
            NUMBER = 4,
            WORD = 5,
            STRING = 6,
            CHARACTER = 7,
            CLASSNAME = 8,
            DEFNAME = 9,
            OPERATOR = 10,
            IDENTIFIER = 11,
            REGEX = 12,
            GLOBAL = 13,
            SYMBOL = 14,
            MODULE_NAME = 15,
            INSTANCE_VAR = 16,
            CLASS_VAR = 17,
            BACKTICKS = 18,
            DATASECTION = 19,
            HERE_DELIM = 20,
            HERE_Q = 21,
            HERE_QQ = 22,
            HERE_QX = 23,
            STRING_Q = 24,
            STRING_QQ = 25,
            STRING_QX = 26,
            STRING_QR = 27,
            STRING_QW = 28,
            WORD_DEMOTED = 29,
            STDIN = 30,
            STDOUT = 31,
            STDERR = 40,
            UPPER_BOUND = 41
        }
    }
}
