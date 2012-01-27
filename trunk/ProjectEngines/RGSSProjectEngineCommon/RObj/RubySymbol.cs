using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class RubySymbol
    {
        private string name;
        private static List<RubySymbol> symbols = new List<RubySymbol>();

        protected RubySymbol(string s)
        {
            this.name = s;
            symbols.Add(this);
        }

        public static List<RubySymbol> GetSymbols()
        {
            return symbols;
        }

        public static RubySymbol GetSymbol(string s)
        {
            foreach (RubySymbol symbol in symbols)
            {
                if (symbol.GetString().Equals(s))
                {
                    return symbol;
                }
            }
            return new RubySymbol(s);
        }

        public string GetString()
        {
            return this.name;
        }

        public override string ToString()
        {
            return (":" + this.name);
        }
    }
}
