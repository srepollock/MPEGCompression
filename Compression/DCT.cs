using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /// <summary>
    /// DCT class
    /// This is used to DCT the data, to show the changes of the image's
    /// pixel data. Everything should be taken in as bytes into forward
    /// DCT and returned as Doubles (to not lose data). And inverse
    /// will take in either Doubles and returned as bytes or sbytes.
    /// </summary>
    public class DCT
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DCT()
        {

        }
        /// <summary>
        /// C
        /// Will check if the number entered is 0, and if it is, then 
        /// returns 1 / sqrt(2).
        /// Otherwise returns 1.
        /// </summary>
        /// <param name="x">Number to be checked</param>
        /// <returns></returns>
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

        /// <summary>
        /// Forward DCT
        /// This will show the changes in the images data. This will only
        /// ever work with blocks of 8x8, so it can be hardcoded to the sum
        /// of E8 E8.
        /// </summary>
        /// <param name="imgData">Image data as an 8x8 block</param>
        /// <returns>double array of doubles</returns>
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
                            temp += Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                                * Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                                * imgData[j,i];
                        }
                    }
                    forwardData[v, u] = temp * ((C(v) * C(u)) / 4);
                }
            }
            return forwardData;
        }
        /// <summary>
        /// Byte Inverse DCT
        /// Reverses forward DCT to undo showing the changes of the data.
        /// Again, it will only work on 8x8 blocks of image data.
        /// </summary>
        /// <param name="dctData">Data that has been DCT'ed</param>
        /// <returns>Double array of Byte data</returns>
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
                            temp += ((C(v) * C(u)))
                                * Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                                * Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                                * dctData[v, u];
                        }
                    }
                    temp = temp / 4;
                    if (temp > 255) temp = 255;
                    if (temp < -0) temp = 0;
                    inverseData[j, i] = Convert.ToByte(temp);
                }
            }
            return inverseData;
        }
        /// <summary>
        /// Double Inverse DCT
        /// Reverses forward DCT to undo showing the changes of the data.
        /// Again, it will only work on 8x8 blocks of image data.
        /// </summary>
        /// <param name="dctData">Data that has been DCT'ed</param>
        /// <returns>Double array of double data</returns>
        public double[,] dinverseDCT(double[,] dctData)
        {
            double[,] inverseData = new double[8, 8];
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    double temp = 0;
                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            temp += ((C(v) * C(u)))
                                * Math.Cos(((2 * j + 1) * v * Math.PI) / 16)
                                * Math.Cos(((2 * i + 1) * u * Math.PI) / 16)
                                * dctData[v,u];
                        }
                    }
                    temp = temp / 4;
                    if (temp > 255) temp = 255;
                    if (temp < 0) temp = 0;
                    inverseData[j, i] = temp;
                }
            }
            return inverseData;
        }
    }
}