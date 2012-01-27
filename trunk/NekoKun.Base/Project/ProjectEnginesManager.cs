using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.Core;

namespace orzTech.NekoKun.Base
{
    public static class ProjectEnginesManager
    {
        public static IProjectEngine CreateProjectEngine(string name, Project project)
        {
            List<Codon> items = AddInTree.GetTreeNode("/NekoKun/ProjectEngines").Codons;

            foreach (Codon item in items)
            {
                if (item.Id.ToLower() == name.ToLower())
                {
                    return (IProjectEngine) item.BuildItem(project, null);
                }
            }
            return null;
        }
    }
}
