using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    public class Marshal
    {
        // Fields
        private static List<object> r_objects = new List<object>();
        private static BinaryReader r_stream;
        private static List<RubySymbol> r_symbols = new List<RubySymbol>();
        private static Dictionary<object, int> w_objects = new Dictionary<object, int>();
        private static BinaryWriter w_stream;
        private static Dictionary<RubySymbol, int> w_symbols = new Dictionary<RubySymbol, int>();
        private static StringBuilder write_str;

        // Methods
        private static void dump(object ob, [Optional, DefaultParameterValue(false)] bool _pass)
        {
            if (!_pass)
            {
                write_str.Append('\x0004');
                write_str.Append('\b');
            }
            if ((((!(ob is int) && !(ob is long)) && (!(ob is RubyNil) && !(ob is bool))) && !(ob is RubySymbol)) && w_objects.ContainsKey(ob))
            {
                write_str.Append('@');  
                writeint((long)w_objects[ob]);
            }
            else if ((ob is long) || (ob is int))
            {
                write_str.Append('i');
                writeint(Convert.ToInt64(ob));
            }
            else if (ob is string)
            {
                string s = (string)ob;
                write_str.Append('"');
                writestr(s);
            }
            else if (ob is RubySymbol)
            {
                RubySymbol symbol = (RubySymbol)ob;
                writesym(symbol);
            }
            else if (ob is RubyNil)
            {
                write_str.Append('0');
            }
            else if (ob is RubyObject)
            {
                writeobject((RubyObject)ob);
            }
            else if (ob is bool)
            {
                write_str.Append(((bool)ob) ? 'T' : 'F');
            }
            else if (ob is RubyTable)
            {
                writeTable((RubyTable)ob);
            }
            else if (ob is ICollection<object>)
            {
                writeList((ICollection<object>)ob);
            }
            else if (ob is Regex)
            {
                writereg((Regex)ob);
            }
            else
            {
                if (!(ob is float) && !(ob is double))
                {
                    throw new Exception("写入时遇到不可辨认的类型：" + ob.GetType().ToString());
                }
                write_float((double)ob);
            }
        }

        public static string Dump(object data, [Optional, DefaultParameterValue(null)] Stream s)
        {
            w_symbols.Clear();
            w_objects.Clear();
            write_str = new StringBuilder();
            dump(data, false);
            if ((s != null) && s.CanWrite)
            {
                w_stream = new BinaryWriter(s);
                string str2 = write_str.ToString();
                for (int i = 0; i < str2.Length; i++)
                {
                    byte num = (byte)str2[i];
                    s.WriteByte(num);
                }
            }
            return write_str.ToString();
        }

        public static object Load(Stream s)
        {
            if (!s.CanRead)
            {
                return null;
            }
            r_symbols.Clear();
            r_objects.Clear();
            r_stream = new BinaryReader(s);
            return read(true);
        }

        private static object read([Optional, DefaultParameterValue(true)] bool _pass)
        {
            object obj2 = null;
            int num5;
            if (_pass)
            {
                r_stream.Read();
                r_stream.Read();
            }
            byte num = r_stream.ReadByte();
            long num2 = 0L;
            int num3 = 0;
            switch (num)
            {
                case 0x40:
                    num3 = (int)readint();
                    if (num3 >= r_objects.Count)
                        return new ArgumentOutOfRangeException("r_Object", num3, "r_Object");
                    return new RubyPointer(r_objects[num3]);

                case 70:
                    return false;

                case 0x49:
                    {
                        RubyExpendObject obj3 = new RubyExpendObject();
                        r_objects.Add(obj3);
                        obj3.real = read(false);
                        num2 = readint();
                        for (num5 = 0; num5 < num2; num5++)
                        {
                            obj3.variables[(RubySymbol)read(false)] = read(false);
                        }
                        obj2 = obj3;
                        break;
                    }
                case 0x2f:
                    {
                        string pattern = readstr();
                        int num4 = r_stream.ReadByte();
                        RegexOptions none = RegexOptions.None;
                        if (num4 >= 4)
                        {
                            none |= RegexOptions.Multiline;
                            num4 -= 4;
                        }
                        if (num4 >= 2)
                        {
                            none |= RegexOptions.IgnorePatternWhitespace;
                            num4 -= 2;
                        }
                        if (num4 >= 1)
                        {
                            none |= RegexOptions.IgnoreCase;
                            num4--;
                        }
                        obj2 = new Regex(pattern, none);
                        r_objects.Add(obj2);
                        break;
                    }
                case 0x30:
                    return new RubyNil();

                case 0x22:
                    obj2 = readstr();
                    break;

                case 0x3a:
                    return readsym();

                case 0x3b:
                    num3 = (int)readint();
                    return r_symbols[num3];

                case 0x54:
                    return true;

                case 0x5b:
                    {
                        num2 = readint();
                        List<object> item = new List<object>((int)num2);
                        r_objects.Add(item);
                        for (num5 = 0; num5 < num2; num5++)
                        {
                            item.Add(read(false));
                        }
                        return item;
                    }
                case 0x66:
                    string str = readstr();
                    if (str.Contains("\0"))
                    {
                        str = str.Remove(str.IndexOf("\0"));
                    }//orzFly
                    obj2 = Convert.ToSingle(str);
                    break;

                case 0x75:
                    {
                        long num6;
                        int num8;
                        int num9;
                        string str2 = ((RubySymbol)read(false)).getStr();
                        if (!(str2 == "Table"))
                        {
                            int num14;
                            int num15;
                            int num16;
                            int num18;
                            int num19;
                            switch (str2)
                            {
                                case "Color":
                                    num6 = readint();
                                    num14 = Convert.ToInt32(r_stream.ReadDouble());
                                    num15 = Convert.ToInt32(r_stream.ReadDouble());
                                    num16 = Convert.ToInt32(r_stream.ReadDouble());
                                    obj2 = Color.FromArgb(Convert.ToInt32(r_stream.ReadDouble()), num14, num15, num16);
                                    r_objects.Add(obj2);
                                    break;

                                case "Rect":
                                    num6 = readint();
                                    num8 = readint4(4);
                                    num9 = readint4(4);
                                    num18 = readint4(4);
                                    num19 = readint4(4);
                                    obj2 = new Rectangle(num8, num9, num18, num19);
                                    r_objects.Add(obj2);
                                    break;

                                case "Tone":
                                    num6 = readint();
                                    obj2 = new RubyTone(Convert.ToInt32(r_stream.ReadDouble()), Convert.ToInt32(r_stream.ReadDouble()), Convert.ToInt32(r_stream.ReadDouble()), Convert.ToInt32(r_stream.ReadDouble()));
                                    r_objects.Add(obj2);
                                    break;
                            }
                            if (obj2 == null) throw new Exception("读取时遇到不支持的用户定义类型:" + str2);
                            return obj2;
                        }
                        num6 = readint();
                        int size = readint4(4);
                        num8 = readint4(4);
                        num9 = readint4(4);
                        int num10 = readint4(4);
                        int num11 = readint4(4);
                        RubyTable table = new RubyTable(size, num8, num9, num10);
                        r_objects.Add(table);
                        for (num5 = 0; num5 < num10; num5++)
                        {
                            for (int i = 0; i < num9; i++)
                            {
                                for (int j = 0; j < num8; j++)
                                {
                                    table.value[j, i, num5] = readint4(2);
                                }
                            }
                        }
                        obj2 = table;
                        break;
                    }
                case 0x7b:
                    {
                        Dictionary<object, object> dictionary = new Dictionary<object, object>();
                        r_objects.Add(dictionary);
                        num2 = readint();
                        num5 = 0;
                        while (num5 < num2)
                        {
                            dictionary[read(false)] = read(false);
                            num5++;
                        }
                        obj2 = dictionary;
                        return obj2;
                    }
                case 0x69:
                    return readint();

                case 0x6f:
                    {
                        RubyObject obj4 = new RubyObject();
                        r_objects.Add(obj4);
                        obj4.class_name = (RubySymbol)read(false);
                        num2 = readint();
                        for (num5 = 0; num5 < num2; num5++)
                        {
                            obj4.variables[(RubySymbol)read(false)] = read(false);
                        }
                        return obj4;
                    }
                default:
                    throw new Exception("读取时遇到不可预知的标识符:" + ((char)num));
            }
            r_objects.Add(obj2);
            return obj2;
        }

        private static long readint()
        {
            long num2;
            int num3;
            int num4;
            sbyte num = r_stream.ReadSByte();
            if (num <= -5)
            {
                return (long)(num + 5);
            }
            if (num < 0)
            {
                num2 = 0L;
                for (num3 = 0; num3 < -num; num3++)
                {
                    num4 = 0xff - r_stream.ReadByte();
                    num2 += num4 << (8 * num3);
                }
                return (-num2 - 1L);
            }
            if (num == 0)
            {
                return 0L;
            }
            if (num <= 4)
            {
                num2 = 0L;
                for (num3 = 0; num3 < num; num3++)
                {
                    num4 = r_stream.ReadByte();
                    num2 += num4 << (8 * num3);
                }
                return num2;
            }
            return (num - 5);
        }

        private static int readint4([Optional, DefaultParameterValue(4)] int d)
        {
            int num = 0;
            for (int i = 0; i < d; i++)
            {
                int num3 = r_stream.ReadByte();
                num += num3 << (8 * i);
            }
            return num;
        }

        private static string readstr()
        {
            long num = readint();
            byte[] bytes = new byte[num];
            for (int i = 0; i < num; i++)
            {
                bytes[i] = r_stream.ReadByte();
            }
            UnicodeEncoding encoding = new UnicodeEncoding();
            UTF8Encoding encoding2 = new UTF8Encoding();
            byte[] buffer2 = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
            return encoding.GetString(buffer2);
        }

        private static RubySymbol readsym()
        {
            RubySymbol item = RubySymbol.GetSymbol(readstr());
            r_symbols.Add(item);
            return item;
        }

        private static string special_num(int num, [Optional, DefaultParameterValue(4)] int length)
        {
            string str = "";
            int num2 = 0;
            while (num != 0)
            {
                str = ((char)(num % 0x100)) + str;
                num = num >> 8;
                num2++;
                if (num2 >= length)
                {
                    break;
                }
            }
            for (int i = 0; i < (length - num2); i++)
            {
                str = '\0' + str;
            }
            return str;
        }

        private static void write_float(double f)
        {
            write_str.Append('f');
            writestr(string.Format("{0:g}", f));
        }

        private static void writearray(object[] ar)
        {
            write_str.Append('[');
            writeint((long)ar.Length);
            foreach (object obj2 in ar)
            {
                dump(obj2, true);
            }
        }

        private static void writeColor(Color c)
        {
            write_str.Append('u');
            writesym(RubySymbol.GetSymbol("Color"));
            writeint(0x20L);
        }

        private static void writeint(long i)
        {
            if (i > 0x7aL)
            {
                write_str.Append((char)((ushort)Math.Ceiling(Math.Log((double)i, 256.0))));
                while (i > 0L)
                {
                    write_str.Append((char)((ushort)(i & 0xffL)));
                    i = i >> 8;
                }
            }
            else if (i > 0L)
            {
                write_str.Append((char)((ushort)(i + 5L)));
            }
            else if (i == 0L)
            {
                write_str.Append('\0');
            }
            else if (i >= -123L)
            {
                write_str.Append((char)((ushort)(i - 5L)));
            }
            else
            {
                long num = ~i;
                write_str.Append((char)((ushort)((sbyte)(256.0 - Math.Ceiling(Math.Log((double)num, 256.0))))));
                while (num > 0L)
                {
                    write_str.Append((char)((ushort)((0xffL - num) & 0xffL)));
                    num = num >> 8;
                }
            }
        }

        private static void writeList(ICollection<object> ar)
        {
            write_str.Append('[');
            writeint((long)ar.Count);
            foreach (object obj2 in ar)
            {
                dump(obj2, true);
            }
        }

        private static void writeobject(RubyObject o)
        {
            write_str.Append('o');
            writesym(o.class_name);
            writeint((long)o.variables.Count);
            foreach (RubySymbol symbol in o.variables.Keys)
            {
                writesym(symbol);
                dump(o.variables[symbol], true);
            }
        }

        private static void writereg(Regex r)
        {
            write_str.Append('/');
            byte num = 0;
            if ((r.Options & RegexOptions.Multiline) == RegexOptions.Multiline)
            {
                num = (byte)(num + 4);
            }
            if ((r.Options & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace)
            {
                num = (byte)(num + 2);
            }
            if ((r.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
            {
                num = (byte)(num + 1);
            }
            writestr(r.ToString());
            write_str.Append((char)num);
        }

        private static void writestr(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            byte[] buffer2 = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
            writeint((long)Helper.TrueStrLength.TrueLength(Encoding.UTF8.GetString(buffer2)));
            foreach (byte num in buffer2)
            {
                write_str.Append((char)num);
            }
        }

        private static void writesym(RubySymbol s)
        {
            if (w_symbols.ContainsKey(s))
            {
                int num = w_symbols[s];
                write_str.Append(';');
                writeint((long)num);
            }
            else
            {
                write_str.Append(':');
                writestr(s.getStr());
                w_symbols[s] = w_symbols.Count;
            }
        }

        private static void writeTable(RubyTable t)
        {
            write_str.Append('u');
            writesym(RubySymbol.GetSymbol("Table"));
            int length = t.value.GetLength(0);
            int num = t.value.GetLength(1);
            int num3 = t.value.GetLength(2);
            writeint((long)((((length * num) * num3) * 2) + 20));
            write_str.Append(special_num(t.size, 4));
            write_str.Append(special_num(length, 4));
            write_str.Append(special_num(num, 4));
            write_str.Append(special_num(num3, 4));
            write_str.Append(special_num((length * num) * num3, 4));
            for (int i = 0; i < num3; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    for (int k = 0; k < length; k++)
                    {
                        write_str.Append(special_num(t.value[k, j, i], 2));
                    }
                }
            }
        }
    }
}
