using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /// <summary>
    /// Motion Vector class
    /// </summary>
    /// <remarks>
    /// This is the class used to calculate the motion vectors for MPEG
    /// </remarks>
    class MotionCompesation
    {
        /// <summary>
        /// Mean Absolute Difference between two macroblocks
        /// </summary>
        /// <remarks>
        /// Calculates the mean absolute difference between two macroblocks
        /// where the goal of the search is to find a vector (i, j) as the 
        /// motion vector MV = (u, v), such that MAD(i, j) is the minimum.
        /// </remarks>
        /// <param name="N">Size of the macroblock</param>
        /// <param name="p">Size of the search area (2 * p + 1)</param>
        /// <param name="C">Target (current) frame</param>
        /// <param name="R">Reference frame</param>
        /// <param name="x">Origin of the macroblock</param>
        /// <param name="y">Origin of the macroblock</param>
        /// <returns>Mean Absolute Difference (double int array)</returns>
        public int[,] MAD(int N, int p, byte[,] C, byte[,] R, int x, int y)
        {
                // (x, y) is the upper left corner of the macroblock
            double diff; // difference calculated
                // p is the size of the search area window
            int[,] madAr = new int[2 * p, 2 * p];
                // N is the size of the MACROBLOCK
            for(int i = -p; i < p; i++)
            {
                for(int j = -p; j < p; j++)
                {
                    diff = 0;
                    for (int k = 0; k < N - 1; k++)
                    {
                        for (int l = 0; l < N - 1; l++)
                        {
                            // C is the Target Frame, R is the Reference Frame
                            diff += Math.Abs(C[x + k, y + 1] - R[x + i + k, y + j + l]);
                        }
                    }
                    madAr[i, j] = (int)(diff * (1 / Math.Pow(N, 2)));
                }
            }
            return madAr;
        }

        /// <summary>
        /// Sequential Motion Vector Search
        /// </summary>
        /// <remarks>
        /// This will sequentiall search the whole (2p + 1) * (2p + 1) window
        /// in the Reference frame.
        /// </remarks>
        /// <param name="p">Size of the search area (2 * p + 1)</param>
        /// <param name="madAr">Mean Average Difference array (already calculated)</param>
        public void seqMVSearch(int p, int[,] madAr)
        {
            int minMAD = int.MaxValue; // Init
            int u = -1, v = -1;
            for(int i = -p; i < p; i++)
            {
                for(int j = -p; j < p; j++)
                {
                    int curMAD = madAr[i, j];
                    if(curMAD < minMAD)
                    {
                        minMAD = curMAD;
                        u = i; // get the coords for MV
                        v = j;
                    }
                }
            }
            if(u == -1 || v == -1)
            {
                // cannot return
            }
            else
            {
                // return MV
            }
        }
    }
}
