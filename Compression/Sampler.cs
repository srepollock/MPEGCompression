using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class Sampler
    {
        public static void upsample(byte[,] org, Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth();
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 2)
                {
                    if ((y + 1) < height)
                        org[x + 1, y] = org[x + 1, y + 1] = org[x, y + 1] = org[x, y];
                    else
                        org[x + 1, y] = org[x, y];
                }
            }
        }

        public static byte[,] subsample(byte[,] org, Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth(),
                hHeight = height / 2,
                hWidth = width / 2;
            byte[,] output = new byte[hWidth, hHeight];
            for (int y = 0, yy = 0; y < height; y += 2, yy++)
            {
                for (int x = 0, xx = 0; x < width; x += 2, xx++)
                {
                    output[xx, yy] = org[x, y];
                }
            }
            return output;
        }

        public static void supsample(sbyte[,] org, Data dataObj)
        {
            int height = dataObj.gHead.getHeight(),
                width = dataObj.gHead.getWidth();
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 2)
                {
                    if ((y + 1) < height)
                        org[x + 1, y] = org[x + 1, y + 1] = org[x, y + 1] = org[x, y];
                    else
                        org[x + 1, y] = org[x, y];
                }
            }
        }

        public static sbyte[,] ssubsample(sbyte[,] org, Data dataObj)
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
