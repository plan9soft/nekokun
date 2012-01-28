﻿using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RubyUserdefinedDumpObject
    {
        private object dumpedObject;
        private RubySymbol className;

        public override string ToString()
        {
            return "#<" + this.className.ToString() + ", dumped object: " + this.dumpedObject.ToString() + ">";
        }

        public object DumpedObject
        {
            get { return dumpedObject; }
            set { dumpedObject = value; }
        }

        public RubySymbol ClassName
        {
            get { return className; }
            set { this.className = value; }
        }
    }
}
