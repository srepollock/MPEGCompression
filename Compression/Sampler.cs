﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class Sampler
    {
        public static byte[,] upsample(byte[,] org, ref Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth();
            byte[,] output = new byte[width, height];
            for (int y = 0, yy = 0; y < height; y += 2, yy++)
            {
                for (int x = 0, xx = 0; x < width; x += 2, xx++)
                {
                    if ((y + 1) < height)
                        output[x + 1, y] = output[x + 1, y + 1] = output[x, y + 1] = output[x, y] = org[xx, yy];
                    else
                        output[x + 1, y] = org[xx, yy];
                }
            }
            return output;
        }

        public static byte[,] subsample(byte[,] org, ref Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth(),
                hHeight = height / 2,
                hWidth = width / 2;
            byte[,] output = new byte[hWidth, hHeight];
            for (int y = 0, yy = 0; y < hHeight; y += 2, yy++)
            {
                for (int x = 0, xx = 0; x < hWidth; x += 2, xx++)
                {
                    output[xx, yy] = org[x, y];
                }
            }
            return output;
        }

        public static sbyte[,] supsample(sbyte[,] org, ref Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth();
            sbyte[,] output = new sbyte[width, height];
            for (int y = 0, yy = 0; y < height; y += 2, yy++)
            {
                for (int x = 0, xx = 0; x < width; x += 2, xx++)
                {
                    if ((y + 1) < height)
                        output[x + 1, y] = output[x + 1, y + 1] = output[x, y + 1] = output[x, y] = org[xx, yy];
                    else
                        output[x + 1, y] = org[xx, yy];
                }
            }
            return output;
        }

        public static sbyte[,] ssubsample(sbyte[,] org, ref Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth(),
                hHeight = height / 2,
                hWidth = width / 2;
            sbyte[,] output = new sbyte[hWidth, hHeight];
            for (int y = 0, yy = 0; y < height; y += 2, yy++)
            {
                for (int x = 0, xx = 0; x < width; x += 2, xx++)
                {
                    output[xx, yy] = org[x, y];
                }
            }
            return output;
        }
    }
}
