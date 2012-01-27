using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public static class Marshal
    {
        public static object Load(Stream input)
        {
            return Load(input, false);
        }

        public static object Load(Stream input, bool TreatStringAsBytes)
        {
            RubyMarshalReader reader = new RubyMarshalReader(input);
            reader.TreatStringAsBytes = TreatStringAsBytes;
            return reader.ReadObject();
        }

        public static void Dump(Stream output, object param)
        {
            throw new NotImplementedException("marshal.dump");
        }
    }
}
