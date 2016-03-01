using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class Blocks
    {
        public byte[,] generate2DBlocks(byte[,] data, int offsetx, int offsety)
        {
            byte[,] output = new byte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = data[offsetx + x, offsety + y];
                }
            }
            return output;
        }

        public sbyte[] generateBlocks(sbyte[] data, int offsetx, int offsety)
        {
            sbyte[] output = new sbyte[8 * 8];
            for (int i = 0; i < 64; i++)
            {
                output[i] = data[i + offsetx * offsety];
            }
            return output;
        }

        public sbyte[] generateBlocks(sbyte[] data, int offset)
        {
            sbyte[] output = new sbyte[8 * 8];
            for (int i = 0; i < 64; i++)
            {
                output[i] = data[i + offset];
            }
            return output;
        }

        public void putback(byte[,] original, byte[,] data, int offsetx, int offsety)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    original[offsetx + x, offsety + y] = data[x, y];
                }
            }
        }

        public void putbacks(sbyte[,] original, sbyte[,] data, int offsetx, int offsety)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    original[offsetx + x, offsety + y] = data[x, y];
                }
            }
        }

        public void putbackd(double[,] original, double[,] data, int offsetx, int offsety)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    original[offsetx + x, offsety + y] = data[x, y];
                }
            }
        }
    }
}
