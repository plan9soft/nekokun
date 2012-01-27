using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.Base
{
    public static class ProjectService
    {
        private static Project activeProject = null;

        public static Project ActiveProject
        {
            get { return activeProject; }
        }

        public static string ProjectFileFilter
        {
            get { return "NekoKun Project (*.nkproj)|*.nkproj"; }
        }

        //public Project CreateNewProject(string )
        //{
        //
        //}

        public static Project OpenProject(string projectFileName)
        {
            if (activeProject == null)
            {
                try
                {
                    return activeProject = new Project(projectFileName);
                }
                catch (Exception ex) {
                    ICSharpCode.Core.MessageService.ShowMessage("sorry, cannot open this project. \n" + ex.ToString());
                }
            }
            return null;
        }

        public static bool CloseActiveProject()
        {
            if (activeProject != null)
            {
                if (activeProject.Close())
                {
                    activeProject = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
