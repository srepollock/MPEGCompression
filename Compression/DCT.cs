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
                return (Math.Sqrt(2) / 2);
            }
            else { // it is not zero
                return 1;
            }
        }

        /*
            Need the image data here
        */
        public double[,] forwardDCT(byte[,] imgData)
        {
            double[,] forwardData = new double[8, 8];
            for(int y = 0; y < 8; y++)
            {
                for(int x = 0; x < 8; x++)
                {
                    forwardData[x,y] = innerForwardDCT(imgData[x, y], x, y);
                }
            }
            return forwardData;
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
                        * Math.Cos(((2 * i + 1) * v * Math.PI) / 16)
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
                    temp += (int)((2 * C(u) * C(v)) / 4) 
                        * Math.Cos(((2 * i + 1) * u * Math.PI) / 16) 
                        * Math.Cos(((2 * i + 1) * v * Math.PI) / 16) 
                        * calc;
                }
            }
            return temp;
        }
    }
}