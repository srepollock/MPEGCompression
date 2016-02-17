using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /** 
  * Double check that i should work with PIxel data or should I work with
  * byte data. There could be some rounding errors here.
  */
    public class DCT
    {

        /**
          * Does the contstructor need to hold anything? What is the purpose of 
          * this class? Should it be an information holder? Or should it just
          * be a worker? (Most likely worker)
          */
        public DCT()
        {

        }

        private double C(int x)
        {
            if (x == 0)
            {
                return (1 / (Math.Sqrt(2)));
            }
            else { // it is not zero
                return 1;
            }
        }

        /*
            Proper??
        */
        public double[,] forwardDCT(byte[,] imgData)
        {
            double[,] forwardData = new double[8, 8];
            double temp = 0;
            for (int v = 0; v < 8; v++)
            {
                for(int u = 0; u < 8; u++)
                {
                    temp = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            temp += Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                                * Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                                * imgData[i,j];
                        }
                    }
                    forwardData[v, u] = temp * ((C(u) * C(v)) / 4);
                }
            }
            return forwardData;
        }

        // testing
        public byte[,] inverseDCTByte(double[,] dctData)
        {
            byte[,] inverseData = new byte[8, 8];
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    double temp = 0;
                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            temp += ((C(u) * C(v)) / 4)
                                * Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                                * Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                                * dctData[u, v];
                        }
                    }
                    if (temp > 255) temp = 255;
                    if (temp < 0) temp = 0;
                    inverseData[j, i] = Convert.ToByte(temp);
                }
            }
            return inverseData;
        }

        public double[,] inverseDCT(byte[,] dctData)
        {
            double[,] inverseData = new double[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    inverseData[x, y] = innerInverseDCT(dctData[x, y], x, y);
                }
            }
            return inverseData;
        }

        /*
            Use this one

            This is going to compression in our 'jpeg' format

            Passing in data (data from src[u, v]
            Using (u, v) as the index of the datas {x, y}

            Should get back doubles. We only convert to bytes after quantization
        */
        public double innerForwardDCT(byte src, int u, int v)
        {
            double temp = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    temp += Math.Cos(((2 * i + 1) * u * Math.PI) / 16) 
                        * Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                        * src;
                }
            }
            return temp * ((2 * C(u) * C(v)) / 4);
        }

        /*
            Use this one

            This is going back from the data calculated (for decompression)

            Passing in data (data from created[u, v]
            Using (u, v) as the index of the data {x, y}
        */
        public double innerInverseDCT(byte calc, int u, int v)
        {
            double temp = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    temp += ((2 * C(u) * C(v)) / 4) 
                        * Math.Cos(((2 * i + 1) * u * Math.PI) / 16) 
                        * Math.Cos(((2 * i + 1) * v * Math.PI) / 16) 
                        * calc;
                }
            }
            return temp;
        }

        // testing
        public byte innerInverseDCT(double calc, int u, int v)
        {
            double temp = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    temp += ((2 * C(u) * C(v)) / 4)
                        * Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                        * Math.Cos(((2 * i + 1) * v * Math.PI) / 16)
                        * calc;
                }
            }
            if(temp > 255)
            {
                temp = 255;
            }
            if(temp < 0)
            {
                temp = 0;
            }
            return Convert.ToByte(temp);
        }
    }
}