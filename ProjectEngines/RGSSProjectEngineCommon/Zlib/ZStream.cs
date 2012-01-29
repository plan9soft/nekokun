// from IronRuby
// License: Apache License 2.0 http://www.apache.org/licenses/
// Modified by orzFly

using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS.Zlib
{
    public class ZStream
    {
        // Fields
        protected byte _bitBucket;
        protected byte _bitCount;
        protected bool _closed;
        protected int _inPos = -1;
        protected readonly List<byte> _inputBuffer;
        protected int _outPos = -1;
        protected readonly List<byte> _outputBuffer;

        // Methods
        public ZStream()
        {
            this._outPos = -1;
            this._inPos = -1;
            this._bitBucket = 0;
            this._bitCount = 0;
            this._inputBuffer = new List<byte>();
            this._outputBuffer = new List<byte>();
        }

        public int Adler()
        {
            throw new NotImplementedException();
        }

        public int AvailIn()
        {
            return (this._inputBuffer.Count - this._inPos);
        }

        public bool Close()
        {
            this._closed = true;
            return this._closed;
        }

        public void DataType()
        {
            throw new NotImplementedException();
        }

        public List<byte> FlushNextIn()
        {
            this._inPos = this._inputBuffer.Count;
            return this._inputBuffer;
        }

        public List<byte> FlushNextOut()
        {
            this._outPos = this._outputBuffer.Count;
            return this._outputBuffer;
        }

        public int GetAvailOut()
        {
            return (this._outputBuffer.Count - this._outPos);
        }

        protected int GetBits(int need)
        {
            int num = this._bitBucket;
            while (this._bitCount < need)
            {
                num |= this._inputBuffer[++this._inPos] << (this._bitCount & 0x1f);
                this._bitCount = (byte)(this._bitCount + 8);
            }
            this._bitBucket = (byte)(num >> need);
            this._bitCount = (byte)(this._bitCount - ((byte)need));
            return (num & ((((int)1) << need) - 1));
        }

        public bool IsClosed()
        {
            return this._closed;
        }

        public void Reset()
        {
            this._outPos = -1;
            this._inPos = -1;
            this._inputBuffer.Clear();
            this._outputBuffer.Clear();
        }

        public int SetAvailOut(int size)
        {
            this._outputBuffer.Capacity = size;
            return this._outputBuffer.Count;
        }

        public int TotalIn()
        {
            return this._inputBuffer.Count;
        }

        public int TotalOut()
        {
            return this._outputBuffer.Count;
        }
    }
}
