﻿using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RubyNil
    {
        private static RubyNil instance;

        private RubyNil() { }

        public override string ToString()
        {
            return "Ruby::Nil";
        }

        public static RubyNil Instance {
            get {
                if (instance == null)
                    instance = new RubyNil();
                return instance; 
            }
        }
    }
}
