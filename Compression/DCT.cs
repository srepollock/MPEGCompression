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
            Use this one

            This is going to compression in our 'jpeg' format

            Passing in data (data from src[u, v]
            Using (u, v) as the index of the datas {x, y}
        */
        public int forward2DDCT(int src, int u, int v)
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
            return (int)(temp * ((2 * C(u) * C(v)) / 4));
        }

        /*
            Use this one

            This is going back from the data calculated (for decompression)

            Passing in data (data from created[u, v]
            Using (u, v) as the index of the data {x, y}
        */
        public int inverse2DDCT(int calc, int u, int v)
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
            return (int)temp;
        }
    }
}