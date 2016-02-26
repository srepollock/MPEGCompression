using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class Quantize
    {
        public sbyte[,] quantizeData(double[,] data, Data dataObj)
        {
            sbyte[,] output = new sbyte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = Convert.ToSByte(Math.Round(data[x, y] / dataObj.chrominance[x, y]));
                }
            }
            return output;
        }

        public double[,] inverseQuantizeData(sbyte[,] data, Data dataObj)
        {
            double[,] output = new double[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = (data[x, y] * dataObj.chrominance[x, y]);
                }
            }
            return output;
        }

        public sbyte[,] quantizeLuma(double[,] data, Data dataObj)
        {
            sbyte[,] output = new sbyte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = Convert.ToSByte(Math.Round((double)(data[x, y] / dataObj.luminance[x, y])));
                }
            }
            return output;
        }

        public double[,] inverseQuantizeLuma(sbyte[,] data, Data dataObj)
        {
            double[,] output = new double[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = data[x, y] * dataObj.luminance[x, y];
                }
            }
            return output;
        }
    }
}
