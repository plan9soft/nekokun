// from IronRuby
// License: Apache License 2.0 http://www.apache.org/licenses/
// Modified by orzFly

using System;
using System.Collections.Generic;
using System.Text;

namespace orzTech.NekoKun.ProjectEngines.RGSS.Zlib
{
    public class Inflate : ZStream
    {
        // Fields
        private HuffmanTree _dynamicDistanceCodes;
        private HuffmanTree _dynamicLengthCodes;
        private HuffmanTree _fixedDistanceCodes;
        private HuffmanTree _fixedLengthCodes;
        private bool _rawDeflate;
        private int _wBits;

        // Methods
        private Inflate()
            : this(15)
        {
        }

        private Inflate(int windowBits)
        {
            this._wBits = windowBits;
            if (this._wBits < 0)
            {
                this._rawDeflate = true;
                this._wBits *= -1;
            }
        }

        public new byte[] Close()
        {
            return this._outputBuffer.ToArray();
        }

        private int Codes(HuffmanTree lengthCodes, HuffmanTree distanceCodes)
        {
            int[] numArray = new int[] { 
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 0x11, 0x13, 0x17, 0x1b, 0x1f, 
            0x23, 0x2b, 0x33, 0x3b, 0x43, 0x53, 0x63, 0x73, 0x83, 0xa3, 0xc3, 0xe3, 0x102
         };
            int[] numArray2 = new int[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
         };
            int[] numArray3 = new int[] { 
            1, 2, 3, 4, 5, 7, 9, 13, 0x11, 0x19, 0x21, 0x31, 0x41, 0x61, 0x81, 0xc1, 
            0x101, 0x181, 0x201, 0x301, 0x401, 0x601, 0x801, 0xc01, 0x1001, 0x1801, 0x2001, 0x3001, 0x4001, 0x6001
         };
            int[] numArray4 = new int[] { 
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 
            7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
         };
            int index = 0;
            while (index != 0x100)
            {
                index = this.Decode(lengthCodes);
                if (index < 0)
                {
                    return index;
                }
                if (index < 0x100)
                {
                    this.SetOrExpand<byte>(base._outputBuffer, ++base._outPos, (byte)index);
                }
                if (index <= 0x100)
                {
                    continue;
                }
                index -= 0x101;
                if (index >= 0x1d)
                {
                    throw new ArgumentException("invalid literal/length or distance code in fixed or dynamic block");
                }
                int num2 = numArray[index] + base.GetBits((byte)numArray2[index]);
                index = this.Decode(distanceCodes);
                if (index < 0)
                {
                    return index;
                }
                int num3 = numArray3[index] + base.GetBits((byte)numArray4[index]);
                if (num3 <= base._outputBuffer.Count)
                {
                    goto Label_013F;
                }
                throw new ArgumentException("distance is too far back in fixed or dynamic block");

            Label_0106:
                this.SetOrExpand<byte>(base._outputBuffer, ++base._outPos, base._outputBuffer[base._outPos - num3]);
                num2--;
            Label_013F:
                if (num2 > 0)
                {
                    goto Label_0106;
                }
            }
            return 0;
        }

        private int ConstructTree(HuffmanTree tree, List<int> lengths, int symbols)
        {
            List<int> list = new List<int>();
            for (int i = 0; i <= 15; i++)
            {
                this.SetOrExpand<int>(tree.Count, i, 0);
            }
            for (int j = 0; j <= symbols; j++)
            {
                List<int> list2;
                int num7;
                (list2 = tree.Count)[num7 = lengths[j]] = list2[num7] + 1;
            }
            if (tree.Count[0] == symbols)
            {
                return 0;
            }
            int num3 = 1;
            for (int k = 1; k <= 15; k++)
            {
                num3 = num3 << 1;
                num3 -= tree.Count[k];
                if (num3 < 0)
                {
                    return num3;
                }
            }
            list.Add(0);
            list.Add(0);
            for (int m = 1; m <= 14; m++)
            {
                list.Add(0);
                list[m + 1] = list[m] + tree.Count[m];
            }
            for (int n = 0; n <= symbols; n++)
            {
                if (lengths[n] != 0)
                {
                    List<int> list3;
                    int num8;
                    this.SetOrExpand<int>(tree.Symbol, list[lengths[n]], n);
                    (list3 = list)[num8 = lengths[n]] = list3[num8] + 1;
                }
            }
            return num3;
        }

        private int Decode(HuffmanTree tree)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            for (int i = 1; i <= 15; i++)
            {
                num |= base.GetBits(1);
                int num5 = tree.Count[i];
                if (num < (num2 + num5))
                {
                    return tree.Symbol[num3 + (num - num2)];
                }
                num3 += num5;
                num2 += num5;
                num2 = num2 << 1;
                num = num << 1;
            }
            return -9;
        }

        private void DynamicCodes()
        {
            byte[] buffer = new byte[] { 
            0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 
            14, 1, 15
         };
            int count = base.GetBits(5) + 0x101;
            int num2 = base.GetBits(5) + 1;
            int num3 = base.GetBits(4) + 4;
            List<int> list = new List<int>();
            this._dynamicLengthCodes = new HuffmanTree();
            this._dynamicDistanceCodes = new HuffmanTree();
            if ((count > 0x11e) || (num2 > 30))
            {
                throw new ArgumentException("too many length or distance codes");
            }
            int index = 0;
            while (index < num3)
            {
                this.SetOrExpand<int>(list, buffer[index], base.GetBits(3));
                index++;
            }
            while (index < 0x13)
            {
                this.SetOrExpand<int>(list, buffer[index], 0);
                index++;
            }
            if (this.ConstructTree(this._dynamicLengthCodes, list, 0x12) != 0)
            {
                throw new ArgumentException("code lengths codes incomplete");
            }
            index = 0;
            while (index < (count + num2))
            {
                int item = this.Decode(this._dynamicLengthCodes);
                if (item < 0x10)
                {
                    this.SetOrExpand<int>(list, index, item);
                    index++;
                    continue;
                }
                int num7 = 0;
                switch (item)
                {
                    case 0x11:
                        item = 3 + base.GetBits(3);
                        break;

                    case 0x12:
                        item = 11 + base.GetBits(7);
                        break;

                    case 0x10:
                        if (index == 0)
                        {
                            throw new ArgumentException("repeat lengths with no first length");
                        }
                        num7 = list[index - 1];
                        item = 3 + base.GetBits(2);
                        break;

                    default:
                        throw new ArgumentException("invalid repeat length code");
                }
                if ((index + item) <= (count + num2))
                {
                    goto Label_018D;
                }
                throw new ArgumentException("repeat more than specified lengths");
            Label_0175:
                this.SetOrExpand<int>(list, index, num7);
                index++;
                item--;
            Label_018D:
                if (item != 0)
                {
                    goto Label_0175;
                }
            }
            int num5 = this.ConstructTree(this._dynamicLengthCodes, list, count - 1);
            if ((num5 < 0) || ((num5 > 0) && ((count - this._dynamicLengthCodes.Count[0]) != 1)))
            {
                throw new ArgumentException("invalid literal/length code lengths");
            }
            list.RemoveRange(0, count);
            num5 = this.ConstructTree(this._dynamicDistanceCodes, list, num2 - 1);
            if ((num5 < 0) || ((num5 > 0) && ((num2 - this._dynamicDistanceCodes.Count[0]) != 1)))
            {
                throw new ArgumentException("invalid distance code lengths");
            }
            this.Codes(this._dynamicLengthCodes, this._dynamicDistanceCodes);
        }

        private void FixedCodes()
        {
            if ((this._fixedLengthCodes == null) && (this._fixedDistanceCodes == null))
            {
                this.GenerateHuffmans();
            }
            this.Codes(this._fixedLengthCodes, this._fixedDistanceCodes);
        }

        private void GenerateHuffmans()
        {
            List<int> lengths = new List<int>(300);
            int num = 0;
            while (num < 0x90)
            {
                lengths.Add(8);
                num++;
            }
            while (num < 0x100)
            {
                lengths.Add(9);
                num++;
            }
            while (num < 280)
            {
                lengths.Add(7);
                num++;
            }
            while (num < 0x120)
            {
                lengths.Add(8);
                num++;
            }
            this._fixedLengthCodes = new HuffmanTree();
            this.ConstructTree(this._fixedLengthCodes, lengths, 0x11f);
            lengths.Clear();
            for (int i = 0; i < 30; i++)
            {
                lengths.Add(5);
            }
            this._fixedDistanceCodes = new HuffmanTree();
            this.ConstructTree(this._fixedDistanceCodes, lengths, 0x1d);
        }

        public static byte[] InflateString(byte[] zstring)
        {
            Inflate self = new Inflate();
            return self.InflateStringReal(zstring);
        }

        private byte[] InflateStringReal(byte[] zstring)
        {
            this._inputBuffer.AddRange(zstring);
            if (!this._rawDeflate)
            {
                byte num = this._inputBuffer[++this._inPos];
                byte num2 = this._inputBuffer[++this._inPos];
                if ((((num << 8) + num2) % 0x1f) != 0)
                {
                    throw new ArgumentException("incorrect header check");
                }
                byte num3 = (byte)(num & 15);
                if (num3 != 8)
                {
                    throw new ArgumentException("unknown compression method");
                }
                byte num4 = (byte)(num >> 4);
                if ((num4 + 8) > this._wBits)
                {
                    throw new ArgumentException("invalid window size");
                }
                if (((num2 & 0x20) >> 5) == 1)
                {
                    this._inPos += 4;
                }
            }
            bool flag2 = false;
            while (!flag2)
            {
                flag2 = this.GetBits(1) == 1;
                switch (((byte)this.GetBits(2)))
                {
                    case 0:
                        this.NoCompression();
                        break;

                    case 1:
                        this.FixedCodes();
                        break;

                    case 2:
                        this.DynamicCodes();
                        break;

                    case 3:
                        throw new ArgumentException("invalid block type");
                }
            }
            return this.Close();
        }

        private void NoCompression()
        {
            base._bitBucket = 0;
            base._bitCount = 0;
            if ((base._inPos + 4) > base._inputBuffer.Count)
            {
                throw new ArgumentException("not enough input to read length code");
            }
            int count = base._inputBuffer[++base._inPos] | (base._inputBuffer[++base._inPos] << 8);
            int num2 = base._inputBuffer[++base._inPos] | (base._inputBuffer[++base._inPos] << 8);
            if (((ushort)count) != ((ushort)~num2))
            {
                throw new ArgumentException("invalid stored block lengths");
            }
            if ((base._inPos + count) > base._inputBuffer.Count)
            {
                throw new ArgumentException("ran out of input");
            }
            base._outputBuffer.AddRange(base._inputBuffer.GetRange(base._inPos + 1, count));
            base._inPos += count;
            base._outPos += count;
        }

        private void SetOrExpand<T>(List<T> list, int index, T item)
        {
            int num = index + 1;
            for (int i = num - list.Count; i > 0; i--)
            {
                T local = default(T);
                list.Add(local);
            }
            list[index] = item;
        }

        // Nested Types
        private sealed class HuffmanTree
        {
            // Fields
            internal readonly List<int> Count = new List<int>();
            internal readonly List<int> Symbol = new List<int>();

            // Methods
            internal HuffmanTree()
            {
            }
        }
    }

}
