﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /// <summary>
    /// Blocks class
    /// This class is specifically for creating 8x8 blocks of data, based on
    /// the offset of the position.
    /// </summary>
    class Blocks
    {
        /// <summary>
        /// Generate 2D Blocks
        /// This will generate 2D blocks of byte data.
        /// </summary>
        /// <param name="data">Original data</param>
        /// <param name="offsetx">X offset</param>
        /// <param name="offsety">Y offset</param>
        /// <returns>8x8 byte data</returns>
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
        /// <summary>
        /// Generate Blocks
        /// This will generate a 1D block of 64 (8x8) of sbyte data.
        /// </summary>
        /// <param name="data">Original sbyte data</param>
        /// <param name="offsetx">X offset</param>
        /// <param name="offsety">Y offset</param>
        /// <returns>64 block of sbyte data</returns>
        public sbyte[] generateBlocks(sbyte[] data, int offsetx, int offsety)
        {
            sbyte[] output = new sbyte[8 * 8];
            for (int i = 0; i < 64; i++)
            {
                output[i] = data[i + offsetx * offsety];
            }
            return output;
        }
        /// <summary>
        /// Generate Blocks
        /// This will generate a block of sbyte data
        /// </summary>
        /// <param name="data">Original sbyte data</param>
        /// <param name="offset">Offset in the 1D array</param>
        /// <returns>64 (8x8 block) of sbyte data</returns>
        public sbyte[] generateBlocks(sbyte[] data, int offset)
        {
            sbyte[] output = new sbyte[8 * 8];
            for (int i = 0; i < 64; i++)
            {
                output[i] = data[i + offset];
            }
            return output;
        }
        /// <summary>
        /// Put Back
        /// This puts the 8x8 block of byte data back into the original 
        /// postion of the data 2D array.
        /// </summary>
        /// <param name="original">Original data array (where we put the data back)</param>
        /// <param name="data">Data to put back into the array</param>
        /// <param name="offsetx">X offset of where to put the data</param>
        /// <param name="offsety">Y offset of where to put the data</param>
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
        /// <summary>
        /// Put Back S
        /// This puts the 8x8 block of sbyte data back into the original data
        /// 2D array.
        /// </summary>
        /// <param name="original">Original data array (where we put the data back)</param>
        /// <param name="data">Data to put back into the array</param>
        /// <param name="offsetx">X offset of where to put the data</param>
        /// <param name="offsety">Y offset of where to put the data</param>
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
        /// <summary>
        /// Put Back D
        /// This puts the 8x8 block of doubles back into the original data
        /// 2D array.
        /// </summary>
        /// <param name="original">Original data array (where we put the data back)</param>
        /// <param name="data">Data to put back into the array</param>
        /// <param name="offsetx">X offset of where to put the data</param>
        /// <param name="offsety">Y offset of where to put the data</param>
        public void putbackd(double[,] original, double[,] data, int offsetx, int offsety)
        {
            for (int y = 0; y < 8; y++)
            {
                {
                    for (int x = 0; x < 8; x++)
                    {
                        original[offsetx + x, offsety + y] = data[x, y];
                    }
                }
            }
        }
    }
}
