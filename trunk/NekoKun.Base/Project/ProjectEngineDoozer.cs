using System;
using System.Collections;
using ICSharpCode.Core;

namespace orzTech.NekoKun.Base
{
    public class ProjectEngineDoozer : IDoozer
	{
		public bool HandleConditions {
			get {
				return false;
			}
		}
		
		public object BuildItem(object caller, Codon codon, ArrayList subItems)
		{
            string className = codon.Properties["class"];
            foreach (Runtime runtime in codon.AddIn.Runtimes)
            {
                object o = runtime.LoadedAssembly.CreateInstance(className,true, System.Reflection.BindingFlags.CreateInstance, null, new object[] {caller}, null, null);
                if (o != null)
                {
                    return o;
                }
            }
            return null;
		}
	}
}
