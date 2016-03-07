﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /// <summary>
    /// Gets the how big the array should become to be "padded" for usage.
    /// </summary>
    /// <remarks>
    /// This class is to get the size of the padding to either pad or read
    /// the padded data by.
    /// Redundent, but cleans up the code. May just put back into the
    /// fileloader class later.
    /// </remarks>
    class Pad
    {
        /// <summary>
        /// Public modified height, widht, padded heght, width
        /// </summary>
        public int modH, modW, padW, padH;

        /// <summary>
        /// Takes in the Data object and saves the padded
        /// data size to the object.
        /// </summary>
        /// <remarks>
        /// Takes in a reference of the data object to read in the
        /// header height and widht of the original image, and sets
        /// the padded height and width of the data object for
        /// later use.
        /// </remarks>
        /// <param name="dataObj">Data object to set the padded data and get the header</param>
        public Pad(ref Data dataObj)
        {
            modH = dataObj.gHead.getHeight() % 16;
            modW = dataObj.gHead.getWidth() % 16; // array is 1 # less for each
            padW = 0;
            padH = 0;
            if (modW != 0 || modH != 0)
            {
                padW = (16 - modW == 16) ? 0 : 16 - modW;
                padH = (16 - modH == 16) ? 0 : 16 - modH;
                dataObj.paddedWidth = dataObj.gHead.getWidth() + padW;
                dataObj.paddedHeight = dataObj.gHead.getHeight() + padH;
            }
            else
            {
                dataObj.paddedWidth = dataObj.gHead.getWidth();
                dataObj.paddedHeight = dataObj.gHead.getHeight();
            }
        }
        /// <summary>
        /// Constructor to create the padding informmation from based on the picture number
        /// </summary>
        /// <param name="dataObj">Data object to read the information to and from.</param>
        /// <param name="picNum">Which picture header information to read from.</param>
        public Pad(ref Data dataObj, int picNum)
        {
            if(picNum == 1)
            {
                modH = dataObj.mv1Head.getHeight() % 16;
                modW = dataObj.mv1Head.getWidth() % 16; // array is 1 # less for each
            }
            else
            {
                modH = dataObj.mv2Head.getHeight() % 16;
                modW = dataObj.mv2Head.getWidth() % 16; // array is 1 # less for each
            }
            padW = 0;
            padH = 0;
            if (modW != 0 || modH != 0)
            {
                padW = (16 - modW == 16) ? 0 : 16 - modW;
                padH = (16 - modH == 16) ? 0 : 16 - modH;
                if (picNum == 1)
                {
                    dataObj.paddedWidth = dataObj.mv1Head.getWidth() + padW;
                    dataObj.paddedHeight = dataObj.mv1Head.getHeight() + padH;
                }
                else
                {
                    dataObj.paddedWidth = dataObj.mv2Head.getWidth() + padW;
                    dataObj.paddedHeight = dataObj.mv2Head.getHeight() + padH;
                }
            }
            else
            {
                if(picNum == 1)
                {
                    dataObj.paddedWidth = dataObj.mv1Head.getWidth();
                    dataObj.paddedHeight = dataObj.mv1Head.getHeight();
                }
                else
                {
                    dataObj.paddedWidth = dataObj.mv2Head.getWidth();
                    dataObj.paddedHeight = dataObj.mv2Head.getHeight();
                }
            }
        }

        /// <summary>
        /// Pads the data taken in, and returns the new sized array.
        /// </summary>
        /// <remarks>
        /// Pad data by a certain width and height. This will ensure
        /// that the data is divisable by 8 and 2. Because the data is cut
        /// in half, and then needs to be cut up into 8x8 blocks, it needs 
        /// to be a mod of 16 (explained in the Pad class).
        /// </remarks>
        /// <param name="data">Original data</param>
        /// <param name="padxby">Pad the width by x amount</param>
        /// <param name="padyby">Pad the height by y amount</param>
        /// <returns>Padded data in a 2D byte array</returns>
        public byte[,] padData(byte[,] data, int padxby, int padyby, Data dataObj)
        {
            int width = dataObj.gHead.getWidth(), height = dataObj.gHead.getHeight();
            byte[,] temp = new byte[width + padxby, height + padyby];
            for (int y = 0; y < height + padyby; y++)
            {
                for (int x = 0; x < width + padxby; x++)
                {
                    if (x >= width && y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else if (x >= width)
                    {
                        temp[x, y] = 0;
                    }
                    else if (y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else
                    {
                        temp[x, y] = data[x, y];
                    }
                }
            }
            return temp;
        }

        /// <summary>
        /// Pads the data inside of the Data object based on the picture number
        /// </summary>
        /// /// <param name="data">Original data.</param>
        /// <param name="padxby">Pad the width by x amount.</param>
        /// <param name="padyby">Pad the height by y amount.</param>
        /// /// <param name="dataObj">Data object to read in the width and height of the image.</param>
        /// <param name="picNum">Picture number to read the width and height from.</param>
        /// <returns>Padded data in a 2D byte array.</returns>
        public byte[,] padData(byte[,] data, int padxby, int padyby, Data dataObj, int picNum)
        {
            int width, height;
            if(picNum == 1)
            {
                width = dataObj.mv1Head.getWidth();
                height = dataObj.mv1Head.getHeight();
            }
            else
            {
                width = dataObj.mv2Head.getWidth();
                height = dataObj.mv2Head.getHeight();
            }

            byte[,] temp = new byte[width + padxby, height + padyby];
            for (int y = 0; y < height + padyby; y++)
            {
                for (int x = 0; x < width + padxby; x++)
                {
                    if (x >= width && y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else if (x >= width)
                    {
                        temp[x, y] = 0;
                    }
                    else if (y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else
                    {
                        temp[x, y] = data[x, y];
                    }
                }
            }
            return temp;
        }

        /// <summary>
        /// Pads the data taken in, and returns the new sized array.
        /// </summary>
        /// <remarks>
        /// Pad data by a certain width and height. This will ensure
        /// that the data is divisable by 8 and 2. Because the data is cut
        /// in half, and then needs to be cut up into 8x8 blocks, it needs 
        /// to be a mod of 16 (explained in the Pad class). This is used on
        /// data that is already halved.
        /// 
        /// * I don't belive I use this class any more *
        /// </remarks>
        /// <param name="data">Original data</param>
        /// <param name="padxby">Pad the width by x amount</param>
        /// <param name="padyby">Pad the height by y amount</param>
        /// <returns></returns>
        public byte[,] cpadData(byte[,] data, int padxby, int padyby, Data dataObj)
        {
            int width = dataObj.gHead.getWidth(), height = dataObj.gHead.getHeight();
            byte[,] temp = new byte[(width + padxby) / 2, (height + padyby) / 2];
            for (int y = 0; y < (height + padyby) / 2; y++)
            {
                for (int x = 0; x < (width + padxby) / 2; x++)
                {
                    if (x >= width / 2 && y >= height / 2)
                    {
                        temp[x, y] = 0;
                    }
                    else if (x >= width / 2)
                    {
                        temp[x, y] = 0;
                    }
                    else if (y >= height / 2)
                    {
                        temp[x, y] = 0;
                    }
                    else
                    {
                        temp[x, y] = data[x, y];
                    }
                }
            }
            return temp;
        }
    }
}
