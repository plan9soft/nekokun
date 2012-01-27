using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    class RubyMarshalReader : IDisposable
    {
        private Stream m_stream;
        private BinaryReader m_reader;
        private List<object> m_objects;
        private List<RubySymbol> m_symbols;
        private bool treatStringAsBytes = false;

        public RubyMarshalReader(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (!input.CanRead)
            {
                throw new ArgumentException("stream cannot read");
            }
            this.m_stream = input;
            this.m_objects = new List<object>();
            this.m_symbols = new List<RubySymbol>();
            this.m_reader = new BinaryReader(m_stream);
            this.m_reader.Read();
            this.m_reader.Read();
        }

        public bool TreatStringAsBytes
        {
            get { return treatStringAsBytes; }
            set { treatStringAsBytes = value; }
        }

        /// <summary>
        /// 从流中读取一个 Object。
        /// </summary>
        /// <returns>读取到的 Object.</returns>
        public object ReadObject()
        {
            byte id = m_reader.ReadByte();
            switch (id)
            {
                case 0x40: // @ Object Reference
                    return m_objects[ReadInt()];

                case 0x3b: // ; Symbol Reference
                    return m_symbols[ReadInt()];

                case 0x30: // 0 NilClass
                    return RubyNil.Instance;

                case 0x54: // T TrueClass
                    return true;

                case 0x46: // F FalseClass
                    return false;

                case 0x69: // i Fixnum
                    return ReadInt();

                case 0x66: // f Float
                    string floatstr = ReadString();
                    if (floatstr.Contains("\0"))
                    {
                        floatstr = floatstr.Remove(floatstr.IndexOf("\0"));
                    }
                    float floatobj = Convert.ToSingle(floatstr);
                    m_objects.Add(floatobj);
                    return floatobj;

                case 0x22: // " String
                    object str;
                    if (!TreatStringAsBytes)
                        str = ReadString();
                    else
                        str = ReadStringAsBytes();
                    m_objects.Add(str);
                    return str;

                case 0x3a: // : Symbol
                    RubySymbol symbol = RubySymbol.GetSymbol(ReadString());
                    m_symbols.Add(symbol);
                    return symbol;

                case 0x5b: // [ Array
                    List<object> array = new List<object>();
                    m_objects.Add(array);
                    int arraycount = ReadInt();
                    for (int i = 0; i < arraycount; i++)
                    {
                        array.Add(ReadObject());
                    }
                    return array;

                case 0x7b: // { Hash
                case 0x7d: // } Hash w/ default value
                    RubyHash hash = new RubyHash();
                    m_objects.Add(hash);
                    int hashcount = ReadInt();
                    for (int i = 0; i < hashcount; i++)
                    {
                        hash[ReadObject()] = ReadObject();
                    }
                    if (id == 0x7d)
                        hash.DefaultValue = ReadObject();
                    return hash;

                case 0x2f: // / Regexp
                    string regexPattern = ReadString();
                    int regexOptionsRuby = m_reader.ReadByte();
                    RegexOptions regexOptions = RegexOptions.None;
                    if (regexOptionsRuby >= 4)
                    {
                        regexOptions |= RegexOptions.Multiline;
                        regexOptionsRuby -= 4;
                    }
                    if (regexOptionsRuby >= 2)
                    {
                        regexOptions |= RegexOptions.IgnorePatternWhitespace;
                        regexOptionsRuby -= 2;
                    }
                    if (regexOptionsRuby >= 1)
                    {
                        regexOptions |= RegexOptions.IgnoreCase;
                        regexOptionsRuby -= 1;
                    }
                    Regex regex = new Regex(regexPattern, regexOptions);
                    m_objects.Add(regex);
                    return regex;

                case 0x6f: // o Object
                    RubyObject robj = new RubyObject();
                    m_objects.Add(robj);
                    robj.ClassName = (RubySymbol)ReadObject();
                    int robjcount = ReadInt();
                    for (int i = 0; i < robjcount; i++)
                    {
                        robj[(RubySymbol)ReadObject()] = ReadObject();
                    }
                    return robj;

                case 0x49: // I Expend Object
                    RubyExpendObject expendobject = new RubyExpendObject();
                    m_objects.Add(expendobject);
                    expendobject.BaseObject = ReadObject();
                    int expendobjectcount = ReadInt();
                    for (int i = 0; i < expendobjectcount; i++)
                    {
                        expendobject[(RubySymbol) ReadObject()] = ReadObject();
                    }
                    return expendobject;

                case 0x6c: // l Bignum
                    int sign = 0;
                    switch (m_reader.ReadByte())
                    {
                        case 0x2b:
                            sign = 1;
                            break;

                        case 0x2d:
                            sign = -1;
                            break;

                        default:
                            sign = 0;
                            break;
                    }
                    int num3 = ReadInt();
                    int index = num3 / 2;
                    int num5 = (num3 + 1) / 2;
                    uint[] data = new uint[num5];
                    for (int i = 0; i < index; i++)
                    {
                        data[i] = m_reader.ReadUInt32();
                    }
                    if (index != num5)
                    {
                        data[index] = m_reader.ReadUInt16();
                    }
                    RubyBignum bignum = new RubyBignum(sign, data);
                    this.m_objects.Add(bignum);
                    return bignum;

                case 0x75: // u
                    string uClassName = ((RubySymbol)ReadObject()).GetString();
                    object uObj = null;
                    //
                    return uObj;

                case 0x63: // c Class
                case 0x6d: // m Module
                case 0x4d: // M Module?
                case 0x55: // U
                case 0x53: // S Struct
                case 0x65: // e
                case 0x43: // C
                default:
                    throw new NotImplementedException("not implemented type identifier: " + id.ToString());
            }
        }

        public void AddObject(object Object)
        {
            this.m_objects.Add(Object);
        }

        public string ReadString()
        {
            int count = ReadInt();
            byte[] bytes = m_reader.ReadBytes(count);
            byte[] buffer = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
            return Encoding.Unicode.GetString(buffer);
        }

        public byte[] ReadStringAsBytes()
        {
            int count = ReadInt();
            return m_reader.ReadBytes(count);
        }

        public int ReadInt()
        {
            sbyte num = m_reader.ReadSByte();
            if (num <= -5)
                return num + 5;
            if (num < 0)
            {
                int output = 0;
                for (int i = 0; i < -num; i++)
                {
                    output += (0xff - m_reader.ReadByte()) << (8*i);
                }
                return (-output - 1);
            }
            if (num == 0)
                return 0;
            if (num <= 4)
            {
                int output = 0;
                for (int i = 0; i < num; i++)
                {
                    output += m_reader.ReadByte() << (8*i);
                }
                return output;
            }
            return (num - 5);
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream stream = this.m_stream;
                this.m_stream = null;
                if (stream != null)
                {
                    stream.Close();
                }
            }
            this.m_stream = null;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }
    }
}
