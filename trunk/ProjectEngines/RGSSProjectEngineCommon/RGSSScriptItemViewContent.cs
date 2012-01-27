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

        public override void Save()
        {
            this.scriptItem.Code = editor.Text;
            this.scriptItem.ScriptFile.Save();
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
    }

    public class CONST
    {
        public const uint CARET_EVEN = 8;
        public const uint CARET_JUMPS = 0x10;
        public const uint CARET_SLOP = 1;
        public const uint CARET_STRICT = 4;
        public const uint CARETSTYLE_BLOCK = 2;
        public const uint CARETSTYLE_INVISIBLE = 0;
        public const uint CARETSTYLE_LINE = 1;
        public const uint EDGE_BACKGROUND = 2;
        public const uint EDGE_LINE = 1;
        public const uint EDGE_NONE = 0;
        public const uint INDIC_BOX = 6;
        public const uint INDIC_CONTAINER = 8;
        public const uint INDIC_DIAGONAL = 3;
        public const uint INDIC_HIDDEN = 5;
        public const uint INDIC_MAX = 0x1f;
        public const uint INDIC_PLAIN = 0;
        public const uint INDIC_ROUNDBOX = 7;
        public const uint INDIC_SQUIGGLE = 1;
        public const uint INDIC_STRIKE = 4;
        public const uint INDIC_TT = 2;
        public const int INDIC0_MASK = 0x20;
        public const int INDIC1_MASK = 0x40;
        public const int INDIC2_MASK = 0x80;
        public const int INDICS_MASK = 0xe0;
        public const int INVALID_POSITION = -1;
        public const uint KEYWORDSET_MAX = 8;
        public const uint MARKER_MAX = 0x1f;
        public const uint SC_ALPHA_NOALPHA = 0x100;
        public const uint SC_ALPHA_OPAQUE = 0xff;
        public const uint SC_ALPHA_TRANSPARENT = 0;
        public const uint SC_CACHE_CARET = 1;
        public const uint SC_CACHE_DOCUMENT = 3;
        public const uint SC_CACHE_NONE = 0;
        public const uint SC_CACHE_PAGE = 2;
        public const uint SC_CASE_LOWER = 2;
        public const uint SC_CASE_MIXED = 0;
        public const uint SC_CASE_UPPER = 1;
        public const int SC_CHARSET_8859_15 = 0x3e8;
        public const int SC_CHARSET_ANSI = 0;
        public const int SC_CHARSET_ARABIC = 0xb2;
        public const int SC_CHARSET_BALTIC = 0xba;
        public const int SC_CHARSET_CHINESEBIG5 = 0x88;
        public const int SC_CHARSET_CYRILLIC = 0x4e3;
        public const int SC_CHARSET_DEFAULT = 1;
        public const int SC_CHARSET_EASTEUROPE = 0xee;
        public const int SC_CHARSET_GB2312 = 0x86;
        public const int SC_CHARSET_GREEK = 0xa1;
        public const int SC_CHARSET_HANGUL = 0x81;
        public const int SC_CHARSET_HEBREW = 0xb1;
        public const int SC_CHARSET_JOHAB = 130;
        public const int SC_CHARSET_MAC = 0x4d;
        public const int SC_CHARSET_OEM = 0xff;
        public const int SC_CHARSET_RUSSIAN = 0xcc;
        public const int SC_CHARSET_SHIFTJIS = 0x80;
        public const int SC_CHARSET_SYMBOL = 2;
        public const int SC_CHARSET_THAI = 0xde;
        public const int SC_CHARSET_TURKISH = 0xa2;
        public const int SC_CHARSET_VIETNAMESE = 0xa3;
        public const uint SC_CP_DBCS = 1;
        public const uint SC_CP_UTF8 = 0xfde9;
        public const int SC_CURSORNORMAL = -1;
        public const int SC_CURSORWAIT = 4;
        public const uint SC_EOL_CR = 1;
        public const uint SC_EOL_CRLF = 0;
        public const uint SC_EOL_LF = 2;
        public const uint SC_FOLDFLAG_BOX = 1;
        public const uint SC_FOLDFLAG_LEVELNUMBERS = 0x40;
        public const uint SC_FOLDFLAG_LINEAFTER_CONTRACTED = 0x10;
        public const uint SC_FOLDFLAG_LINEAFTER_EXPANDED = 8;
        public const uint SC_FOLDFLAG_LINEBEFORE_CONTRACTED = 4;
        public const uint SC_FOLDFLAG_LINEBEFORE_EXPANDED = 2;
        public const uint SC_FOLDLEVELBASE = 0x400;
        public const uint SC_FOLDLEVELBOXFOOTERFLAG = 0x8000;
        public const uint SC_FOLDLEVELBOXHEADERFLAG = 0x4000;
        public const uint SC_FOLDLEVELCONTRACTED = 0x10000;
        public const uint SC_FOLDLEVELHEADERFLAG = 0x2000;
        public const uint SC_FOLDLEVELNUMBERMASK = 0xfff;
        public const uint SC_FOLDLEVELUNINDENT = 0x20000;
        public const uint SC_FOLDLEVELWHITEFLAG = 0x1000;
        public const uint SC_LASTSTEPINUNDOREDO = 0x100;
        public const uint SC_MARGIN_NUMBER = 1;
        public const uint SC_MARGIN_SYMBOL = 0;
        public const uint SC_MARK_ARROW = 2;
        public const uint SC_MARK_ARROWDOWN = 6;
        public const uint SC_MARK_ARROWS = 0x18;
        public const uint SC_MARK_BACKGROUND = 0x16;
        public const uint SC_MARK_BOXMINUS = 14;
        public const uint SC_MARK_BOXMINUSCONNECTED = 15;
        public const uint SC_MARK_BOXPLUS = 12;
        public const uint SC_MARK_BOXPLUSCONNECTED = 13;
        public const uint SC_MARK_CHARACTER = 0x2710;
        public const uint SC_MARK_CIRCLE = 0;
        public const uint SC_MARK_CIRCLEMINUS = 20;
        public const uint SC_MARK_CIRCLEMINUSCONNECTED = 0x15;
        public const uint SC_MARK_CIRCLEPLUS = 0x12;
        public const uint SC_MARK_CIRCLEPLUSCONNECTED = 0x13;
        public const uint SC_MARK_DOTDOTDOT = 0x17;
        public const uint SC_MARK_EMPTY = 5;
        public const uint SC_MARK_FULLRECT = 0x1a;
        public const uint SC_MARK_LCORNER = 10;
        public const uint SC_MARK_LCORNERCURVE = 0x10;
        public const uint SC_MARK_MINUS = 7;
        public const uint SC_MARK_PIXMAP = 0x19;
        public const uint SC_MARK_PLUS = 8;
        public const uint SC_MARK_ROUNDRECT = 1;
        public const uint SC_MARK_SHORTARROW = 4;
        public const uint SC_MARK_SMALLRECT = 3;
        public const uint SC_MARK_TCORNER = 11;
        public const uint SC_MARK_TCORNERCURVE = 0x11;
        public const uint SC_MARK_VLINE = 9;
        public const int SC_MARKNUM_FOLDER = 30;
        public const int SC_MARKNUM_FOLDEREND = 0x19;
        public const int SC_MARKNUM_FOLDERMIDTAIL = 0x1b;
        public const int SC_MARKNUM_FOLDEROPEN = 0x1f;
        public const int SC_MARKNUM_FOLDEROPENMID = 0x1a;
        public const int SC_MARKNUM_FOLDERSUB = 0x1d;
        public const int SC_MARKNUM_FOLDERTAIL = 0x1c;
        public const int SC_MASK_FOLDERS = -33554432;
        public const uint SC_MOD_BEFOREDELETE = 0x800;
        public const uint SC_MOD_BEFOREINSERT = 0x400;
        public const uint SC_MOD_CHANGEFOLD = 8;
        public const uint SC_MOD_CHANGEINDICATOR = 0x4000;
        public const uint SC_MOD_CHANGEMARKER = 0x200;
        public const uint SC_MOD_CHANGESTYLE = 4;
        public const uint SC_MOD_DELETETEXT = 2;
        public const uint SC_MOD_INSERTTEXT = 1;
        public const int SC_MODEVENTMASKALL = 0x6fff;
        public const uint SC_MULTILINEUNDOREDO = 0x1000;
        public const uint SC_MULTISTEPUNDOREDO = 0x80;
        public const uint SC_PERFORMED_REDO = 0x40;
        public const uint SC_PERFORMED_UNDO = 0x20;
        public const uint SC_PERFORMED_USER = 0x10;
        public const uint SC_PRINT_BLACKONWHITE = 2;
        public const uint SC_PRINT_COLOURONWHITE = 3;
        public const uint SC_PRINT_COLOURONWHITEDEFAULTBG = 4;
        public const uint SC_PRINT_INVERTLIGHT = 1;
        public const uint SC_PRINT_NORMAL = 0;
        public const uint SC_SEL_LINES = 2;
        public const uint SC_SEL_RECTANGLE = 1;
        public const uint SC_SEL_STREAM = 0;
        public const uint SC_TIME_FOREVER = 0x989680;
        public const uint SC_WRAP_CHAR = 2;
        public const uint SC_WRAP_NONE = 0;
        public const uint SC_WRAP_WORD = 1;
        public const uint SC_WRAPVISUALFLAG_END = 1;
        public const uint SC_WRAPVISUALFLAG_NONE = 0;
        public const uint SC_WRAPVISUALFLAG_START = 2;
        public const uint SC_WRAPVISUALFLAGLOC_DEFAULT = 0;
        public const uint SC_WRAPVISUALFLAGLOC_END_BY_TEXT = 1;
        public const uint SC_WRAPVISUALFLAGLOC_START_BY_TEXT = 2;
        public const uint SCEN_CHANGE = 0x300;
        public const uint SCEN_KILLFOCUS = 0x100;
        public const uint SCEN_SETFOCUS = 0x200;
        public const uint SCFIND_MATCHCASE = 4;
        public const uint SCFIND_POSIX = 0x400000;
        public const uint SCFIND_REGEXP = 0x200000;
        public const uint SCFIND_WHOLEWORD = 2;
        public const uint SCFIND_WORDSTART = 0x100000;
        public const uint SCFIND_INALLSCRIPTPAGES = 8;         //  rgeseditor add !
        public const uint SCI_ADDREFDOCUMENT = 0x948;
        public const uint SCI_ADDSTYLEDTEXT = 0x7d2;
        public const uint SCI_ADDTEXT = 0x7d1;
        public const uint SCI_ALLOCATE = 0x98e;
        public const uint SCI_APPENDTEXT = 0x8ea;
        public const uint SCI_ASSIGNCMDKEY = 0x816;
        public const uint SCI_AUTOCACTIVE = 0x836;
        public const uint SCI_AUTOCCANCEL = 0x835;
        public const uint SCI_AUTOCCOMPLETE = 0x838;
        public const uint SCI_AUTOCGETAUTOHIDE = 0x847;
        public const uint SCI_AUTOCGETCANCELATSTART = 0x83f;
        public const uint SCI_AUTOCGETCHOOSESINGLE = 0x842;
        public const uint SCI_AUTOCGETCURRENT = 0x98d;
        public const uint SCI_AUTOCGETDROPRESTOFWORD = 0x8df;
        public const uint SCI_AUTOCGETIGNORECASE = 0x844;
        public const uint SCI_AUTOCGETMAXHEIGHT = 0x8a3;
        public const uint SCI_AUTOCGETMAXWIDTH = 0x8a1;
        public const uint SCI_AUTOCGETSEPARATOR = 0x83b;
        public const uint SCI_AUTOCGETTYPESEPARATOR = 0x8ed;
        public const uint SCI_AUTOCPOSSTART = 0x837;
        public const uint SCI_AUTOCSELECT = 0x83c;
        public const uint SCI_AUTOCSETAUTOHIDE = 0x846;
        public const uint SCI_AUTOCSETCANCELATSTART = 0x83e;
        public const uint SCI_AUTOCSETCHOOSESINGLE = 0x841;
        public const uint SCI_AUTOCSETDROPRESTOFWORD = 0x8de;
        public const uint SCI_AUTOCSETFILLUPS = 0x840;
        public const uint SCI_AUTOCSETIGNORECASE = 0x843;
        public const uint SCI_AUTOCSETMAXHEIGHT = 0x8a2;
        public const uint SCI_AUTOCSETMAXWIDTH = 0x8a0;
        public const uint SCI_AUTOCSETSEPARATOR = 0x83a;
        public const uint SCI_AUTOCSETTYPESEPARATOR = 0x8ee;
        public const uint SCI_AUTOCSHOW = 0x834;
        public const uint SCI_AUTOCSTOPS = 0x839;
        public const uint SCI_BACKTAB = 0x918;
        public const uint SCI_BEGINUNDOACTION = 0x81e;
        public const uint SCI_BRACEBADLIGHT = 0x930;
        public const uint SCI_BRACEHIGHLIGHT = 0x92f;
        public const uint SCI_BRACEMATCH = 0x931;
        public const uint SCI_CALLTIPACTIVE = 0x89a;
        public const uint SCI_CALLTIPCANCEL = 0x899;
        public const uint SCI_CALLTIPPOSSTART = 0x89b;
        public const uint SCI_CALLTIPSETBACK = 0x89d;
        public const uint SCI_CALLTIPSETFORE = 0x89e;
        public const uint SCI_CALLTIPSETFOREHLT = 0x89f;
        public const uint SCI_CALLTIPSETHLT = 0x89c;
        public const uint SCI_CALLTIPSHOW = 0x898;
        public const uint SCI_CALLTIPUSESTYLE = 0x8a4;
        public const uint SCI_CANCEL = 0x915;
        public const uint SCI_CANPASTE = 0x87d;
        public const uint SCI_CANREDO = 0x7e0;
        public const uint SCI_CANUNDO = 0x87e;
        public const uint SCI_CHARLEFT = 0x900;
        public const uint SCI_CHARLEFTEXTEND = 0x901;
        public const uint SCI_CHARLEFTRECTEXTEND = 0x97c;
        public const uint SCI_CHARRIGHT = 0x902;
        public const uint SCI_CHARRIGHTEXTEND = 0x903;
        public const uint SCI_CHARRIGHTRECTEXTEND = 0x97d;
        public const uint SCI_CHOOSECARETX = 0x95f;
        public const uint SCI_CLEAR = 0x884;
        public const uint SCI_CLEARALL = 0x7d4;
        public const uint SCI_CLEARALLCMDKEYS = 0x818;
        public const uint SCI_CLEARCMDKEY = 0x817;
        public const uint SCI_CLEARDOCUMENTSTYLE = 0x7d5;
        public const uint SCI_CLEARREGISTEREDIMAGES = 0x968;
        public const uint SCI_COLOURISE = 0xfa3;
        public const uint SCI_CONVERTEOLS = 0x7ed;
        public const uint SCI_COPY = 0x882;
        public const uint SCI_COPYRANGE = 0x973;
        public const uint SCI_COPYTEXT = 0x974;
        public const uint SCI_CREATEDOCUMENT = 0x947;
        public const uint SCI_CUT = 0x881;
        public const uint SCI_DELETEBACK = 0x916;
        public const uint SCI_DELETEBACKNOTLINE = 0x928;
        public const uint SCI_DELLINELEFT = 0x95b;
        public const uint SCI_DELLINERIGHT = 0x95c;
        public const uint SCI_DELWORDLEFT = 0x91f;
        public const uint SCI_DELWORDRIGHT = 0x920;
        public const uint SCI_DOCLINEFROMVISIBLE = 0x8ad;
        public const uint SCI_DOCUMENTEND = 0x90e;
        public const uint SCI_DOCUMENTENDEXTEND = 0x90f;
        public const uint SCI_DOCUMENTSTART = 0x90c;
        public const uint SCI_DOCUMENTSTARTEXTEND = 0x90d;
        public const uint SCI_EDITTOGGLEOVERTYPE = 0x914;
        public const uint SCI_EMPTYUNDOBUFFER = 0x87f;
        public const uint SCI_ENCODEDFROMUTF8 = 0x991;
        public const uint SCI_ENDUNDOACTION = 0x81f;
        public const uint SCI_ENSUREVISIBLE = 0x8b8;
        public const uint SCI_ENSUREVISIBLEENFORCEPOLICY = 0x8ba;
        public const uint SCI_FINDCOLUMN = 0x998;
        public const uint SCI_FINDTEXT = 0x866;
        public const uint SCI_FORMATRANGE = 0x867;
        public const uint SCI_FORMFEED = 0x91a;
        public const uint SCI_GETANCHOR = 0x7d9;
        public const uint SCI_GETBACKSPACEUNINDENTS = 0x8d7;
        public const uint SCI_GETBUFFEREDDRAW = 0x7f2;
        public const uint SCI_GETCARETFORE = 0x85a;
        public const uint SCI_GETCARETLINEBACK = 0x831;
        public const uint SCI_GETCARETLINEBACKALPHA = 0x9a7;
        public const uint SCI_GETCARETLINEVISIBLE = 0x82f;
        public const uint SCI_GETCARETPERIOD = 0x81b;
        public const uint SCI_GETCARETSTICKY = 0x999;
        public const uint SCI_GETCARETSTYLE = 0x9d1;
        public const uint SCI_GETCARETWIDTH = 0x88d;
        public const uint SCI_GETCHARAT = 0x7d7;
        public const uint SCI_GETCODEPAGE = 0x859;
        public const uint SCI_GETCOLUMN = 0x851;
        public const uint SCI_GETCONTROLCHARSYMBOL = 0x955;
        public const uint SCI_GETCURLINE = 0x7eb;
        public const uint SCI_GETCURRENTPOS = 0x7d8;
        public const uint SCI_GETCURSOR = 0x953;
        public const uint SCI_GETDIRECTFUNCTION = 0x888;
        public const uint SCI_GETDIRECTPOINTER = 0x889;
        public const uint SCI_GETDOCPOINTER = 0x935;
        public const uint SCI_GETEDGECOLOUR = 0x93c;
        public const uint SCI_GETEDGECOLUMN = 0x938;
        public const uint SCI_GETEDGEMODE = 0x93a;
        public const uint SCI_GETENDATLASTLINE = 0x8e6;
        public const uint SCI_GETENDSTYLED = 0x7ec;
        public const uint SCI_GETEOLMODE = 0x7ee;
        public const uint SCI_GETFIRSTVISIBLELINE = 0x868;
        public const uint SCI_GETFOCUS = 0x94d;
        public const uint SCI_GETFOLDEXPANDED = 0x8b6;
        public const uint SCI_GETFOLDLEVEL = 0x8af;
        public const uint SCI_GETFOLDPARENT = 0x8b1;
        public const uint SCI_GETHIGHLIGHTGUIDE = 0x857;
        public const uint SCI_GETHOTSPOTACTIVEBACK = 0x9bf;
        public const uint SCI_GETHOTSPOTACTIVEFORE = 0x9be;
        public const uint SCI_GETHOTSPOTACTIVEUNDERLINE = 0x9c0;
        public const uint SCI_GETHOTSPOTSINGLELINE = 0x9c1;
        public const uint SCI_GETHSCROLLBAR = 0x853;
        public const uint SCI_GETINDENT = 0x84b;
        public const uint SCI_GETINDENTATIONGUIDES = 0x855;
        public const uint SCI_GETINDICATORCURRENT = 0x9c5;
        public const uint SCI_GETINDICATORVALUE = 0x9c7;
        public const uint SCI_GETLASTCHILD = 0x8b0;
        public const uint SCI_GETLAYOUTCACHE = 0x8e1;
        public const uint SCI_GETLENGTH = 0x7d6;
        public const uint SCI_GETLEXER = 0xfa2;
        public const uint SCI_GETLINE = 0x869;
        public const uint SCI_GETLINECOUNT = 0x86a;
        public const uint SCI_GETLINEENDPOSITION = 0x858;
        public const uint SCI_GETLINEINDENTATION = 0x84f;
        public const uint SCI_GETLINEINDENTPOSITION = 0x850;
        public const uint SCI_GETLINESELENDPOSITION = 0x979;
        public const uint SCI_GETLINESELSTARTPOSITION = 0x978;
        public const uint SCI_GETLINESTATE = 0x82d;
        public const uint SCI_GETLINEVISIBLE = 0x8b4;
        public const uint SCI_GETMARGINLEFT = 0x86c;
        public const uint SCI_GETMARGINMASKN = 0x8c5;
        public const uint SCI_GETMARGINRIGHT = 0x86e;
        public const uint SCI_GETMARGINSENSITIVEN = 0x8c7;
        public const uint SCI_GETMARGINTYPEN = 0x8c1;
        public const uint SCI_GETMARGINWIDTHN = 0x8c3;
        public const uint SCI_GETMAXLINESTATE = 0x82e;
        public const uint SCI_GETMODEVENTMASK = 0x94a;
        public const uint SCI_GETMODIFY = 0x86f;
        public const uint SCI_GETMOUSEDOWNCAPTURES = 0x951;
        public const uint SCI_GETMOUSEDWELLTIME = 0x8d9;
        public const uint SCI_GETOVERTYPE = 0x88b;
        public const uint SCI_GETPASTECONVERTENDINGS = 0x9a4;
        public const uint SCI_GETPOSITIONCACHE = 0x9d3;
        public const uint SCI_GETPRINTCOLOURMODE = 0x865;
        public const uint SCI_GETPRINTMAGNIFICATION = 0x863;
        public const uint SCI_GETPRINTWRAPMODE = 0x967;
        public const uint SCI_GETPROPERTY = 0xfa8;
        public const uint SCI_GETPROPERTYEXPANDED = 0xfa9;
        public const uint SCI_GETPROPERTYINT = 0xfaa;
        public const uint SCI_GETREADONLY = 0x85c;
        public const uint SCI_GETSCROLLWIDTH = 0x8e3;
        public const uint SCI_GETSEARCHFLAGS = 0x897;
        public const uint SCI_GETSELECTIONEND = 0x861;
        public const uint SCI_GETSELECTIONMODE = 0x977;
        public const uint SCI_GETSELECTIONSTART = 0x85f;
        public const uint SCI_GETSELTEXT = 0x871;
        public const uint SCI_GETSTATUS = 0x94f;
        public const uint SCI_GETSTYLEAT = 0x7da;
        public const uint SCI_GETSTYLEBITS = 0x82b;
        public const uint SCI_GETSTYLEBITSNEEDED = 0xfab;
        public const uint SCI_GETSTYLEDTEXT = 0x7df;
        public const uint SCI_GETTABINDENTS = 0x8d5;
        public const uint SCI_GETTABWIDTH = 0x849;
        public const uint SCI_GETTARGETEND = 0x891;
        public const uint SCI_GETTARGETSTART = 0x88f;
        public const uint SCI_GETTEXT = 0x886;
        public const uint SCI_GETTEXTLENGTH = 0x887;
        public const uint SCI_GETTEXTRANGE = 0x872;
        public const uint SCI_GETTWOPHASEDRAW = 0x8eb;
        public const uint SCI_GETUNDOCOLLECTION = 0x7e3;
        public const uint SCI_GETUSEPALETTE = 0x85b;
        public const uint SCI_GETUSETABS = 0x84d;
        public const uint SCI_GETVIEWEOL = 0x933;
        public const uint SCI_GETVIEWWS = 0x7e4;
        public const uint SCI_GETVSCROLLBAR = 0x8e9;
        public const uint SCI_GETWRAPMODE = 0x8dd;
        public const uint SCI_GETWRAPSTARTINDENT = 0x9a1;
        public const uint SCI_GETWRAPVISUALFLAGS = 0x99d;
        public const uint SCI_GETWRAPVISUALFLAGSLOCATION = 0x99f;
        public const uint SCI_GETXOFFSET = 0x95e;
        public const uint SCI_GETZOOM = 0x946;
        public const uint SCI_GOTOLINE = 0x7e8;
        public const uint SCI_GOTOPOS = 0x7e9;
        public const uint SCI_GRABFOCUS = 0x960;
        public const uint SCI_HIDELINES = 0x8b3;
        public const uint SCI_HIDESELECTION = 0x873;
        public const uint SCI_HOME = 0x908;
        public const uint SCI_HOMEDISPLAY = 0x929;
        public const uint SCI_HOMEDISPLAYEXTEND = 0x92a;
        public const uint SCI_HOMEEXTEND = 0x909;
        public const uint SCI_HOMERECTEXTEND = 0x97e;
        public const uint SCI_HOMEWRAP = 0x92d;
        public const uint SCI_HOMEWRAPEXTEND = 0x992;
        public const uint SCI_INDICATORALLONFOR = 0x9ca;
        public const uint SCI_INDICATORCLEARRANGE = 0x9c9;
        public const uint SCI_INDICATOREND = 0x9cd;
        public const uint SCI_INDICATORFILLRANGE = 0x9c8;
        public const uint SCI_INDICATORSTART = 0x9cc;
        public const uint SCI_INDICATORVALUEAT = 0x9cb;
        public const uint SCI_INDICGETFORE = 0x823;
        public const uint SCI_INDICGETSTYLE = 0x821;
        public const uint SCI_INDICGETUNDER = 0x9cf;
        public const uint SCI_INDICSETFORE = 0x822;
        public const uint SCI_INDICSETSTYLE = 0x820;
        public const uint SCI_INDICSETUNDER = 0x9ce;
        public const uint SCI_INSERTTEXT = 0x7d3;
        public const uint SCI_LEXER_START = 0xfa0;
        public const uint SCI_LINECOPY = 0x997;
        public const uint SCI_LINECUT = 0x921;
        public const uint SCI_LINEDELETE = 0x922;
        public const uint SCI_LINEDOWN = 0x8fc;
        public const uint SCI_LINEDOWNEXTEND = 0x8fd;
        public const uint SCI_LINEDOWNRECTEXTEND = 0x97a;
        public const uint SCI_LINEDUPLICATE = 0x964;
        public const uint SCI_LINEEND = 0x90a;
        public const uint SCI_LINEENDDISPLAY = 0x92b;
        public const uint SCI_LINEENDDISPLAYEXTEND = 0x92c;
        public const uint SCI_LINEENDEXTEND = 0x90b;
        public const uint SCI_LINEENDRECTEXTEND = 0x980;
        public const uint SCI_LINEENDWRAP = 0x993;
        public const uint SCI_LINEENDWRAPEXTEND = 0x994;
        public const uint SCI_LINEFROMPOSITION = 0x876;
        public const uint SCI_LINELENGTH = 0x92e;
        public const uint SCI_LINESCROLL = 0x878;
        public const uint SCI_LINESCROLLDOWN = 0x926;
        public const uint SCI_LINESCROLLUP = 0x927;
        public const uint SCI_LINESJOIN = 0x8f0;
        public const uint SCI_LINESONSCREEN = 0x942;
        public const uint SCI_LINESSPLIT = 0x8f1;
        public const uint SCI_LINETRANSPOSE = 0x923;
        public const uint SCI_LINEUP = 0x8fe;
        public const uint SCI_LINEUPEXTEND = 0x8ff;
        public const uint SCI_LINEUPRECTEXTEND = 0x97b;
        public const uint SCI_LOADLEXERLIBRARY = 0xfa7;
        public const uint SCI_LOWERCASE = 0x924;
        public const uint SCI_MARKERADD = 0x7fb;
        public const uint SCI_MARKERADDSET = 0x9a2;
        public const uint SCI_MARKERDEFINE = 0x7f8;
        public const uint SCI_MARKERDEFINEPIXMAP = 0x801;
        public const uint SCI_MARKERDELETE = 0x7fc;
        public const uint SCI_MARKERDELETEALL = 0x7fd;
        public const uint SCI_MARKERDELETEHANDLE = 0x7e2;
        public const uint SCI_MARKERGET = 0x7fe;
        public const uint SCI_MARKERLINEFROMHANDLE = 0x7e1;
        public const uint SCI_MARKERNEXT = 0x7ff;
        public const uint SCI_MARKERPREVIOUS = 0x800;
        public const uint SCI_MARKERSETALPHA = 0x9ac;
        public const uint SCI_MARKERSETBACK = 0x7fa;
        public const uint SCI_MARKERSETFORE = 0x7f9;
        public const uint SCI_MOVECARETINSIDEVIEW = 0x961;
        public const uint SCI_NEWLINE = 0x919;
        public const uint SCI_NULL = 0x87c;
        public const uint SCI_OPTIONAL_START = 0xbb8;
        public const uint SCI_PAGEDOWN = 0x912;
        public const uint SCI_PAGEDOWNEXTEND = 0x913;
        public const uint SCI_PAGEDOWNRECTEXTEND = 0x982;
        public const uint SCI_PAGEUP = 0x910;
        public const uint SCI_PAGEUPEXTEND = 0x911;
        public const uint SCI_PAGEUPRECTEXTEND = 0x981;
        public const uint SCI_PARADOWN = 0x96d;
        public const uint SCI_PARADOWNEXTEND = 0x96e;
        public const uint SCI_PARAUP = 0x96f;
        public const uint SCI_PARAUPEXTEND = 0x970;
        public const uint SCI_PASTE = 0x883;
        public const uint SCI_POINTXFROMPOSITION = 0x874;
        public const uint SCI_POINTYFROMPOSITION = 0x875;
        public const uint SCI_POSITIONAFTER = 0x972;
        public const uint SCI_POSITIONBEFORE = 0x971;
        public const uint SCI_POSITIONFROMLINE = 0x877;
        public const uint SCI_POSITIONFROMPOINT = 0x7e6;
        public const uint SCI_POSITIONFROMPOINTCLOSE = 0x7e7;
        public const uint SCI_REDO = 0x7db;
        public const uint SCI_REGISTERIMAGE = 0x965;
        public const uint SCI_RELEASEDOCUMENT = 0x949;
        public const uint SCI_REPLACESEL = 0x87a;
        public const uint SCI_REPLACETARGET = 0x892;
        public const uint SCI_REPLACETARGETRE = 0x893;
        public const uint SCI_SCROLLCARET = 0x879;
        public const uint SCI_SEARCHANCHOR = 0x93e;
        public const uint SCI_SEARCHINTARGET = 0x895;
        public const uint SCI_SEARCHNEXT = 0x93f;
        public const uint SCI_SEARCHPREV = 0x940;
        public const uint SCI_SELECTALL = 0x7dd;
        public const uint SCI_SELECTIONDUPLICATE = 0x9a5;
        public const uint SCI_SELECTIONISRECTANGLE = 0x944;
        public const uint SCI_SETANCHOR = 0x7ea;
        public const uint SCI_SETBACKSPACEUNINDENTS = 0x8d6;
        public const uint SCI_SETBUFFEREDDRAW = 0x7f3;
        public const uint SCI_SETCARETFORE = 0x815;
        public const uint SCI_SETCARETLINEBACK = 0x832;
        public const uint SCI_SETCARETLINEBACKALPHA = 0x9a6;
        public const uint SCI_SETCARETLINEVISIBLE = 0x830;
        public const uint SCI_SETCARETPERIOD = 0x81c;
        public const uint SCI_SETCARETSTICKY = 0x99a;
        public const uint SCI_SETCARETSTYLE = 0x9d0;
        public const uint SCI_SETCARETWIDTH = 0x88c;
        public const uint SCI_SETCHARSDEFAULT = 0x98c;
        public const uint SCI_SETCODEPAGE = 0x7f5;
        public const uint SCI_SETCONTROLCHARSYMBOL = 0x954;
        public const uint SCI_SETCURRENTPOS = 0x85d;
        public const uint SCI_SETCURSOR = 0x952;
        public const uint SCI_SETDOCPOINTER = 0x936;
        public const uint SCI_SETEDGECOLOUR = 0x93d;
        public const uint SCI_SETEDGECOLUMN = 0x939;
        public const uint SCI_SETEDGEMODE = 0x93b;
        public const uint SCI_SETENDATLASTLINE = 0x8e5;
        public const uint SCI_SETEOLMODE = 0x7ef;
        public const uint SCI_SETFOCUS = 0x94c;
        public const uint SCI_SETFOLDEXPANDED = 0x8b5;
        public const uint SCI_SETFOLDFLAGS = 0x8b9;
        public const uint SCI_SETFOLDLEVEL = 0x8ae;
        public const uint SCI_SETFOLDMARGINCOLOUR = 0x8f2;
        public const uint SCI_SETFOLDMARGINHICOLOUR = 0x8f3;
        public const uint SCI_SETHIGHLIGHTGUIDE = 0x856;
        public const uint SCI_SETHOTSPOTACTIVEBACK = 0x96b;
        public const uint SCI_SETHOTSPOTACTIVEFORE = 0x96a;
        public const uint SCI_SETHOTSPOTACTIVEUNDERLINE = 0x96c;
        public const uint SCI_SETHOTSPOTSINGLELINE = 0x975;
        public const uint SCI_SETHSCROLLBAR = 0x852;
        public const uint SCI_SETINDENT = 0x84a;
        public const uint SCI_SETINDENTATIONGUIDES = 0x854;
        public const uint SCI_SETINDICATORCURRENT = 0x9c4;
        public const uint SCI_SETINDICATORVALUE = 0x9c6;
        public const uint SCI_SETKEYWORDS = 0xfa5;
        public const uint SCI_SETLAYOUTCACHE = 0x8e0;
        public const uint SCI_SETLENGTHFORENCODE = 0x990;
        public const uint SCI_SETLEXER = 0xfa1;
        public const uint SCI_SETLEXERLANGUAGE = 0xfa6;
        public const uint SCI_SETLINEINDENTATION = 0x84e;
        public const uint SCI_SETLINESTATE = 0x82c;
        public const uint SCI_SETMARGINLEFT = 0x86b;
        public const uint SCI_SETMARGINMASKN = 0x8c4;
        public const uint SCI_SETMARGINRIGHT = 0x86d;
        public const uint SCI_SETMARGINSENSITIVEN = 0x8c6;
        public const uint SCI_SETMARGINTYPEN = 0x8c0;
        public const uint SCI_SETMARGINWIDTHN = 0x8c2;
        public const uint SCI_SETMODEVENTMASK = 0x937;
        public const uint SCI_SETMOUSEDOWNCAPTURES = 0x950;
        public const uint SCI_SETMOUSEDWELLTIME = 0x8d8;
        public const uint SCI_SETOVERTYPE = 0x88a;
        public const uint SCI_SETPASTECONVERTENDINGS = 0x9a3;
        public const uint SCI_SETPOSITIONCACHE = 0x9d2;
        public const uint SCI_SETPRINTCOLOURMODE = 0x864;
        public const uint SCI_SETPRINTMAGNIFICATION = 0x862;
        public const uint SCI_SETPRINTWRAPMODE = 0x966;
        public const uint SCI_SETPROPERTY = 0xfa4;
        public const uint SCI_SETREADONLY = 0x87b;
        public const uint SCI_SETSAVEPOINT = 0x7de;
        public const uint SCI_SETSCROLLWIDTH = 0x8e2;
        public const uint SCI_SETSEARCHFLAGS = 0x896;
        public const uint SCI_SETSEL = 0x870;
        public const uint SCI_SETSELBACK = 0x814;
        public const uint SCI_SETSELECTIONEND = 0x860;
        public const uint SCI_SETSELECTIONMODE = 0x976;
        public const uint SCI_SETSELECTIONSTART = 0x85e;
        public const uint SCI_SETSELFORE = 0x813;
        public const uint SCI_SETSTATUS = 0x94e;
        public const uint SCI_SETSTYLEBITS = 0x82a;
        public const uint SCI_SETSTYLING = 0x7f1;
        public const uint SCI_SETSTYLINGEX = 0x819;
        public const uint SCI_SETTABINDENTS = 0x8d4;
        public const uint SCI_SETTABWIDTH = 0x7f4;
        public const uint SCI_SETTARGETEND = 0x890;
        public const uint SCI_SETTARGETSTART = 0x88e;
        public const uint SCI_SETTEXT = 0x885;
        public const uint SCI_SETTWOPHASEDRAW = 0x8ec;
        public const uint SCI_SETUNDOCOLLECTION = 0x7dc;
        public const uint SCI_SETUSEPALETTE = 0x7f7;
        public const uint SCI_SETUSETABS = 0x84c;
        public const uint SCI_SETVIEWEOL = 0x934;
        public const uint SCI_SETVIEWWS = 0x7e5;
        public const uint SCI_SETVISIBLEPOLICY = 0x95a;
        public const uint SCI_SETVSCROLLBAR = 0x8e8;
        public const uint SCI_SETWHITESPACEBACK = 0x825;
        public const uint SCI_SETWHITESPACECHARS = 0x98b;
        public const uint SCI_SETWHITESPACEFORE = 0x824;
        public const uint SCI_SETWORDCHARS = 0x81d;
        public const uint SCI_SETWRAPMODE = 0x8dc;
        public const uint SCI_SETWRAPSTARTINDENT = 0x9a0;
        public const uint SCI_SETWRAPVISUALFLAGS = 0x99c;
        public const uint SCI_SETWRAPVISUALFLAGSLOCATION = 0x99e;
        public const uint SCI_SETXCARETPOLICY = 0x962;
        public const uint SCI_SETXOFFSET = 0x95d;
        public const uint SCI_SETYCARETPOLICY = 0x963;
        public const uint SCI_SETZOOM = 0x945;
        public const uint SCI_SHOWLINES = 0x8b2;
        public const uint SCI_START = 0x7d0;
        public const uint SCI_STARTRECORD = 0xbb9;
        public const uint SCI_STARTSTYLING = 0x7f0;
        public const uint SCI_STOPRECORD = 0xbba;
        public const uint SCI_STUTTEREDPAGEDOWN = 0x985;
        public const uint SCI_STUTTEREDPAGEDOWNEXTEND = 0x986;
        public const uint SCI_STUTTEREDPAGEUP = 0x983;
        public const uint SCI_STUTTEREDPAGEUPEXTEND = 0x984;
        public const uint SCI_STYLECLEARALL = 0x802;
        public const uint SCI_STYLEGETBACK = 0x9b2;
        public const uint SCI_STYLEGETBOLD = 0x9b3;
        public const uint SCI_STYLEGETCASE = 0x9b9;
        public const uint SCI_STYLEGETCHANGEABLE = 0x9bc;
        public const uint SCI_STYLEGETCHARACTERSET = 0x9ba;
        public const uint SCI_STYLEGETEOLFILLED = 0x9b7;
        public const uint SCI_STYLEGETFONT = 0x9b6;
        public const uint SCI_STYLEGETFORE = 0x9b1;
        public const uint SCI_STYLEGETHOTSPOT = 0x9bd;
        public const uint SCI_STYLEGETITALIC = 0x9b4;
        public const uint SCI_STYLEGETSIZE = 0x9b5;
        public const uint SCI_STYLEGETUNDERLINE = 0x9b8;
        public const uint SCI_STYLEGETVISIBLE = 0x9bb;
        public const uint SCI_STYLERESETDEFAULT = 0x80a;
        public const uint SCI_STYLESETBACK = 0x804;
        public const uint SCI_STYLESETBOLD = 0x805;
        public const uint SCI_STYLESETCASE = 0x80c;
        public const uint SCI_STYLESETCHANGEABLE = 0x833;
        public const uint SCI_STYLESETCHARACTERSET = 0x812;
        public const uint SCI_STYLESETEOLFILLED = 0x809;
        public const uint SCI_STYLESETFONT = 0x808;
        public const uint SCI_STYLESETFORE = 0x803;
        public const uint SCI_STYLESETHOTSPOT = 0x969;
        public const uint SCI_STYLESETITALIC = 0x806;
        public const uint SCI_STYLESETSIZE = 0x807;
        public const uint SCI_STYLESETUNDERLINE = 0x80b;
        public const uint SCI_STYLESETVISIBLE = 0x81a;
        public const uint SCI_TAB = 0x917;
        public const uint SCI_TARGETASUTF8 = 0x98f;
        public const uint SCI_TARGETFROMSELECTION = 0x8ef;
        public const uint SCI_TEXTHEIGHT = 0x8e7;
        public const uint SCI_TEXTWIDTH = 0x8e4;
        public const uint SCI_TOGGLECARETSTICKY = 0x99b;
        public const uint SCI_TOGGLEFOLD = 0x8b7;
        public const uint SCI_UNDO = 0x880;
        public const uint SCI_UPPERCASE = 0x925;
        public const uint SCI_USEPOPUP = 0x943;
        public const uint SCI_USERLISTSHOW = 0x845;
        public const uint SCI_VCHOME = 0x91b;
        public const uint SCI_VCHOMEEXTEND = 0x91c;
        public const uint SCI_VCHOMERECTEXTEND = 0x97f;
        public const uint SCI_VCHOMEWRAP = 0x995;
        public const uint SCI_VCHOMEWRAPEXTEND = 0x996;
        public const uint SCI_VISIBLEFROMDOCLINE = 0x8ac;
        public const uint SCI_WORDENDPOSITION = 0x8db;
        public const uint SCI_WORDLEFT = 0x904;
        public const uint SCI_WORDLEFTEND = 0x987;
        public const uint SCI_WORDLEFTENDEXTEND = 0x988;
        public const uint SCI_WORDLEFTEXTEND = 0x905;
        public const uint SCI_WORDPARTLEFT = 0x956;
        public const uint SCI_WORDPARTLEFTEXTEND = 0x957;
        public const uint SCI_WORDPARTRIGHT = 0x958;
        public const uint SCI_WORDPARTRIGHTEXTEND = 0x959;
        public const uint SCI_WORDRIGHT = 0x906;
        public const uint SCI_WORDRIGHTEND = 0x989;
        public const uint SCI_WORDRIGHTENDEXTEND = 0x98a;
        public const uint SCI_WORDRIGHTEXTEND = 0x907;
        public const uint SCI_WORDSTARTPOSITION = 0x8da;
        public const uint SCI_WRAPCOUNT = 0x8bb;
        public const uint SCI_ZOOMIN = 0x91d;
        public const uint SCI_ZOOMOUT = 0x91e;
        public const uint SCK_ADD = 310;
        public const uint SCK_BACK = 8;
        public const uint SCK_DELETE = 0x134;
        public const uint SCK_DIVIDE = 0x138;
        public const uint SCK_DOWN = 300;
        public const uint SCK_END = 0x131;
        public const uint SCK_ESCAPE = 7;
        public const uint SCK_HOME = 0x130;
        public const uint SCK_INSERT = 0x135;
        public const uint SCK_LEFT = 0x12e;
        public const uint SCK_NEXT = 0x133;
        public const uint SCK_PRIOR = 0x132;
        public const uint SCK_RETURN = 13;
        public const uint SCK_RIGHT = 0x12f;
        public const uint SCK_SUBTRACT = 0x137;
        public const uint SCK_TAB = 9;
        public const uint SCK_UP = 0x12d;
        public const uint SCMOD_ALT = 4;
        public const uint SCMOD_CTRL = 2;
        public const uint SCMOD_NORM = 0;
        public const uint SCMOD_SHIFT = 1;
        public const uint SCN_AUTOCSELECTION = 0x7e6;
        public const uint SCN_CALLTIPCLICK = 0x7e5;
        public const uint SCN_CHARADDED = 0x7d1;
        public const uint SCN_DOUBLECLICK = 0x7d6;
        public const uint SCN_DWELLEND = 0x7e1;
        public const uint SCN_DWELLSTART = 0x7e0;
        public const uint SCN_HOTSPOTCLICK = 0x7e3;
        public const uint SCN_HOTSPOTDOUBLECLICK = 0x7e4;
        public const uint SCN_INDICATORCLICK = 0x7e7;
        public const uint SCN_INDICATORRELEASE = 0x7e8;
        public const uint SCN_KEY = 0x7d5;
        public const uint SCN_MACRORECORD = 0x7d9;
        public const uint SCN_MARGINCLICK = 0x7da;
        public const uint SCN_MODIFIED = 0x7d8;
        public const uint SCN_MODIFYATTEMPTRO = 0x7d4;
        public const uint SCN_NEEDSHOWN = 0x7db;
        public const uint SCN_PAINTED = 0x7dd;
        public const uint SCN_SAVEPOINTLEFT = 0x7d3;
        public const uint SCN_SAVEPOINTREACHED = 0x7d2;
        public const uint SCN_STYLENEEDED = 0x7d0;
        public const uint SCN_UPDATEUI = 0x7d7;
        public const uint SCN_URIDROPPED = 0x7df;
        public const uint SCN_USERLISTSELECTION = 0x7de;
        public const uint SCN_ZOOM = 0x7e2;
        public const uint SCWS_INVISIBLE = 0;
        public const uint SCWS_VISIBLEAFTERINDENT = 2;
        public const uint SCWS_VISIBLEALWAYS = 1;
        public const int STYLE_BRACEBAD = 0x23;
        public const int STYLE_BRACELIGHT = 0x22;
        public const int STYLE_CALLTIP = 0x26;
        public const int STYLE_CONTROLCHAR = 0x24;
        public const int STYLE_DEFAULT = 0x20;
        public const int STYLE_INDENTGUIDE = 0x25;
        public const int STYLE_LASTPREDEFINED = 0x27;
        public const int STYLE_LINENUMBER = 0x21;
        public const int STYLE_MAX = 0x7f;
        public const uint VISIBLE_SLOP = 1;
        public const uint VISIBLE_STRICT = 4;

        public const int SC_IV_NONE = 0;
        public const int SC_IV_REAL = 1;
        public const int SC_IV_LOOKFORWARD = 2;
        public const int SC_IV_LOOKBOTH = 3;

        //  winuser.h
        public const uint EM_EMPTYUNDOBUFFER = 0x00CD;
    }

    public enum SCE_RB
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
