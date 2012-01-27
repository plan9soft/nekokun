using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RubyModule
    {
        private string name;
        private static List<RubyModule> modules = new List<RubyModule>();

        protected RubyModule(string s)
        {
            this.name = s;
            modules.Add(this);
        }

        public static List<RubyModule> GetModules()
        {
            return modules;
        }

        public static RubyModule GetModule(string s)
        {
            foreach (RubyModule module in modules)
            {
                if (module.ToString().Equals(s))
                {
                    return module;
                }
            }
            return new RubyModule(s);
        }

        public override string ToString()
        {
            return (this.name);
        }
    }
}
