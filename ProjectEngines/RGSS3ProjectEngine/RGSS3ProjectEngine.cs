using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using orzTech.NekoKun.Base;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RGSS3ProjectEngine : AbstractProjectEngine
    {

        ScriptEngine rubyEngine;
        ScriptScope rubyScope;
        List<object> eventCommands;
        bool eventSupport;
        List<object> databases;
        bool editorScriptFailed;
        string editorScriptFileName;
        RGSSScriptFile editorScripts;
        RGSSScriptFile runtimeScripts;
        string runtimePrefix;
        string runtimeFilename;
        string runtimeScriptsFilename;
        string runtimeSettingsFilename;
        ProjectItem runtimeScriptsProjectItem;
        ProjectItem editorScriptsProjectItem;

        public RGSS3ProjectEngine(Project project)
            : base(project)
        {
            editorScriptFailed = false;
            try
            {
                LoadEditorScript();
            }
            catch
            {
                editorScriptFailed = true;
                ClearEngine();
            }
            LoadScript();

            this.editorScriptsProjectItem = new ProjectItem("Editor Script", this.editorScripts);
            this.runtimeScriptsProjectItem = new ProjectItem("Runtime Script", this.runtimeScripts);

            this.Project.ProjectExplorer.AddItem(this.editorScriptsProjectItem);
            this.Project.ProjectExplorer.AddItem(this.runtimeScriptsProjectItem);

            foreach (RGSSScriptItem item in this.runtimeScripts)
            {
                this.Project.ProjectExplorer.AddItem(new ProjectItem(item.Title, item));
            }
        }

        private void LoadScript()
        {
            this.runtimePrefix = System.IO.Path.Combine(this.Project.ProjectDirectory, this.Project.ProjectSettingsXMLNode.SelectNodes("RGSS3Runtime/@fileprefix")[0].InnerText);
            this.runtimeFilename = this.runtimePrefix + ".exe";
            this.runtimeSettingsFilename =  this.runtimePrefix + ".ini";
            this.runtimeScriptsFilename = System.IO.Path.Combine(this.Project.ProjectDirectory, FileService.IniReadValue(this.runtimeSettingsFilename, "Game", "Scripts"));
            this.runtimeScripts = RGSSScriptFile.LoadFile(this.runtimeScriptsFilename);
            this.Project.ProjectTitle = FileService.IniReadValue(this.runtimeSettingsFilename, "Game", "Title");
            
            this.Project.AddFile(this.runtimeSettingsFilename);
            this.Project.AddFile(this.runtimeScriptsFilename);
        }

        public string EditorScriptFileName
        {
            get { return this.editorScriptFileName; }
        }

        public void Says(object str)
        {
            ICSharpCode.Core.MessageService.ShowMessage(str.ToString());
        }

        private void LoadEditorScript()
        {
            ClearEngine();
            this.editorScriptFileName = System.IO.Path.Combine(this.Project.ProjectDirectory, this.Project.ProjectSettingsXMLNode.SelectNodes("EditorScript/@filename")[0].InnerText);

            try
            {
                rubyEngine = Ruby.CreateEngine();
                rubyScope = rubyEngine.CreateScope();
                rubyScope.SetVariable("editor", this);
                rubyEngine.Execute("load_assembly 'IronRuby.Libraries', 'IronRuby.StandardLibrary.Zlib'", rubyScope);
            }
            catch { }

            this.editorScripts = RGSSScriptFile.LoadFile(this.editorScriptFileName);
            this.Project.AddFile(this.editorScriptFileName);

            foreach (RGSSScriptItem item in this.editorScripts)
            {
                try
                {
                    rubyEngine.Execute(item.Code, rubyScope);
                }
                catch (Exception ex)
                {
                    ICSharpCode.Core.MessageService.ShowMessage("error in editor script: " + item.Title.ToString() + "\n" + ex.Message);
                    break;
                }
            }
        }

        private void ClearEngine()
        {
            // throw new NotImplementedException();
        }
    }

}
