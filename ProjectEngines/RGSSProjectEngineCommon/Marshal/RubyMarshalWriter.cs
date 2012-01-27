using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace orzTech.NekoKun.ProjectEngines.RGSS
{
    class RubyMarshalWriter
    {
        private Stream m_stream;
        private BinaryWriter m_writer;
        private List<object> m_objects;
        private List<RubySymbol> m_symbols;

        public RubyMarshalWriter(Stream output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (!output.CanWrite)
            {
                throw new ArgumentException("stream cannot write");
            }
            this.m_stream = output;
            this.m_objects = new List<object>();
            this.m_symbols = new List<RubySymbol>();
            this.m_writer = new BinaryWriter(m_stream);
        }

        public void Dump(object obj)
        {
            this.m_writer.Write((byte)4);
            this.m_writer.Write((byte)8);
            WriteAnObject(obj);
            this.m_stream.Flush();
        }

        public void WriteAnObject(object obj)
        {
            if (obj is int)
            {
                int num = (int)obj;
                if ((num < -1073741824) || (num >= 0x40000000))
                {
                    obj = num;
                }
            }
            if (obj == null || obj is RubyNil)
            {
                this.m_writer.Write((byte)0x30);
            }
            else if (obj is bool)
            {
                this.m_writer.Write(((bool)obj) ? ((byte)0x54) : ((byte)70));
            }
            else if (obj is int)
            {
                this.WriteFixnum((int)obj);
            }
            else if (obj is RubySymbol)
            {
                this.WriteSymbol((RubySymbol)obj);
            }
            else if (obj is string)
            {
                this.WriteString((string)obj);
            }
            else if (obj is byte[])
            {
                this.WriteString((byte[])obj);
            }
            else if (obj is List<object>)
            {
                this.WriteArray((List<object>)obj);
            }
            else
            {
                throw new ArgumentException("i don't know how to marshal.dump this type: " + obj.GetType().FullName);
            }
        }

        private void WriteArray(List<object> value)
        {
            this.m_writer.Write((byte)0x5b);
            this.WriteInt32(value.Count);
            foreach (object obj2 in value)
            {
                this.WriteAnObject(obj2);
            }
        }
        private void WriteString(byte[] bytes)
        {
            this.m_writer.Write((byte)0x22);
            this.WriteStringValue(bytes);
        }
        private void WriteString(string value)
        {
            this.m_writer.Write((byte)0x22);
            this.WriteStringValue(value);
        }
        private void WriteStringValue(string value)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            byte[] buffer = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
            this.WriteInt32(buffer.Length);
            this.m_writer.Write(buffer);
        }
        private void WriteStringValue(byte[] value)
        {
            this.WriteInt32(value.Length);
            this.m_writer.Write(value);
        }
        private void WriteSymbol(RubySymbol value)
        {
            if (this.m_symbols.Contains(value))
            {
                this.m_writer.Write((byte)0x3b);
                this.WriteInt32(this.m_symbols.IndexOf(value));
            }
            else
            {
                this.m_symbols.Add(value);
                this.m_writer.Write((byte)0x3a);
                this.WriteStringValue(value.GetString());
            }
        }
        private void WriteFixnum(int value)
        {
            this.m_writer.Write((byte)0x69);
            this.WriteInt32(value);
        }
        private void WriteInt32(int value)
        {
            if (value == 0)
            {
                this.m_writer.Write((byte)0);
            }
            else if ((value > 0) && (value < 0x7b))
            {
                this.m_writer.Write((byte)(value + 5));
            }
            else if ((value < 0) && (value > -124))
            {
                this.m_writer.Write((sbyte)(value - 5));
            }
            else
            {
                sbyte num2;
                byte[] buffer = new byte[5];
                buffer[1] = (byte)(value & 0xff);
                buffer[2] = (byte)((value >> 8) & 0xff);
                buffer[3] = (byte)((value >> 0x10) & 0xff);
                buffer[4] = (byte)((value >> 0x18) & 0xff);
                int index = 4;
                if (value >= 0)
                {
                    while (buffer[index] == 0)
                    {
                        index--;
                    }
                    num2 = (sbyte)index;
                }
                else
                {
                    while (buffer[index] == 0xff)
                    {
                        index--;
                    }
                    num2 = (sbyte)-index;
                }
                buffer[0] = (byte)num2;
                this.m_writer.Write(buffer, 0, index + 1);
            }
        }
    }
}
