using System;
using System.Collections.Generic;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public static class Helper
    {
        // Methods
        public static string Array_String(object[] ar)
        {
            string str = "[";
            foreach (object obj2 in ar)
            {
                str = str + ((obj2.GetType() == ar.GetType()) ? Array_String((object[])obj2) : obj2.ToString()) + ",";
            }
            return (str + "]");
        }

        public static string Dictionary_String(Dictionary<object, object> d)
        {
            string str = "{";
            foreach (object obj2 in d.Keys)
            {
                object obj3 = str;
                str = string.Concat(new object[] { obj3, obj2, " => ", d[obj2], " , " });
            }
            return (str + "}");
        }

        // Nested Types
        public static class TrueStrLength
        {
            // Methods
            public static string CutTrueLength(string strOriginal, int maxTrueLength, char chrPad, bool blnCutTail)
            {
                int num2;
                string str = strOriginal;
                if ((strOriginal == null) || (maxTrueLength <= 0))
                {
                    return "";
                }
                int num = TrueLength(strOriginal);
                if (num > maxTrueLength)
                {
                    if (blnCutTail)
                    {
                        for (num2 = strOriginal.Length - 1; num2 > 0; num2--)
                        {
                            str = str.Substring(0, num2);
                            if (TrueLength(str) == maxTrueLength)
                            {
                                return str;
                            }
                            if (TrueLength(str) < maxTrueLength)
                            {
                                return (str + chrPad.ToString());
                            }
                        }
                    }
                    return str;
                }
                for (num2 = 0; num2 < (maxTrueLength - num); num2++)
                {
                    str = str + chrPad.ToString();
                }
                return str;
            }

            public static int TrueLength(string str)
            {
                int num = 0;
                int length = str.Length;
                for (int i = 0; i < length; i++)
                {
                    int num3 = Convert.ToChar(str.Substring(i, 1));
                    if ((num3 < 0) || (num3 > 0x7f))
                    {
                        num += 3;
                    }
                    else
                    {
                        num++;
                    }
                }
                return num;
            }
        }
    }
}
