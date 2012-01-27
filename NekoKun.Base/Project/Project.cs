using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace orzTech.NekoKun.Base
{
    public class Project
    {
        string projectFileName = "";
        XmlDocument projectXMLDocument;
        XmlNode projectSettingsXMLNode;
        IProjectEngine engine;
        ProjectExplorer projectExplorer;
        List<string> projectFiles;
        string projectTitle;
        

        public Project(string projectFileName)
        {
            this.projectFiles = new List<string>();
            this.projectFileName = projectFileName;
            this.projectXMLDocument = new XmlDocument();
            this.projectXMLDocument.Load(projectFileName);
            this.projectSettingsXMLNode = projectXMLDocument.SelectNodes("/NekoKunProject")[0];
            this.projectTitle = System.IO.Path.GetFileNameWithoutExtension(this.projectFileName);
            this.projectExplorer = new ProjectExplorer(this); 

            engine = ProjectEnginesManager.CreateProjectEngine(this.projectSettingsXMLNode.Attributes["engine"].InnerText, this);

            Workbench.Instance.ShowView(new ProjectExplorerViewContent(this));
        }

        public ProjectExplorer ProjectExplorer
        {
            get { return this.projectExplorer; }
        }

        public string ProjectTitle
        {
            get { return this.projectTitle; }
            set { 
                this.projectTitle = value; 
            }
        }

        public string ProjectFileName
        {
            get { return this.projectFileName; }
        }

        public string ProjectDirectory
        {
            get { return System.IO.Path.GetDirectoryName(this.projectFileName); }
        }

        public XmlNode ProjectSettingsXMLNode
        {
            get { return projectSettingsXMLNode; }
        }

        public void Save()
        {
        }

        public bool Close()
        {
            return true;
        }

        public void AddFile(string FileName)
        {
            string file = null;
            try
            {
                file = System.IO.Path.GetFullPath(FileName);
            }
            catch { return; }
            if (file != null)
            {
                if (this.projectFiles.Contains(file))
                    this.projectFiles.Add(file);
            }
        }
    }
}
