using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class ZigZag
    {
        public sbyte[] zigzag(sbyte[,] data)
        {
            // testing
            /*
            byte[,] data = new byte[,]
            {
                {0,1,5,6,14,15,27,28 },
                {2,4,7,13,16,26,29,42 },
                {3,8,12,17,25,30,41,43 },
                {9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };
            */
            sbyte[] result = new sbyte[8 * 8];

            int i = 0,
                x = 0,
                y = 0,
                d = 0; // -1 for the to-right move, +1 for the bottom-left move
            bool flag = false;
            bool reverseFlag = false;

            do
            {
                flag = false;
                if (x > 7 || reverseFlag)
                {
                    d++;
                    y = d;
                    reverseFlag = true;
                    if (x > 7) x--;
                    result[i] = data[x, y];
                    while (x != d)
                    {
                        result[++i] = data[--x, ++y];
                    }
                    if (++d > 7) break;
                    x = d;
                    result[++i] = data[x, y];
                    while (y != d)
                    {
                        result[++i] = data[++x, --y];
                    }
                    i++;
                }
                else
                {
                    result[i] = data[x, y];
                    if (i == 0) x++;
                    while (x != 0)
                    {
                        result[++i] = data[--x, ++y];
                    }
                    while (y != 0)
                    {
                        if (flag || y == 1) x++;
                        else y += 2;
                        result[++i] = data[x, --y];
                        if (!flag) flag = true;
                    }
                    x++; y = 0; i++;
                }
            } while (i < 64);

            result[63] = data[7, 7];

            return result;
        }

        public sbyte[,] unzigzag(sbyte[] data)
        {
            // testing
            /*
            byte[,] data = new byte[,]
            {
                {0,1,5,6,14,15,27,28 },
                {2,4,7,13,16,26,29,42 },
                {3,8,12,17,25,30,41,43 },
                {9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };
            */
            sbyte[,] result = new sbyte[8, 8];

            int i = 0,
                x = 0,
                y = 0,
                d = 0; // -1 for the to-right move, +1 for the bottom-left move
            bool flag = false;
            bool reverseFlag = false;

            do
            {
                flag = false;
                if (x >= 8 || reverseFlag)
                {
                    d++;
                    y = d;
                    reverseFlag = true;
                    if (x >= 8)
                        x--;
                    result[x, y] = data[i];
                    while (x != d)
                    {
                        result[--x, ++y] = data[++i];
                    }
                    if (++d > 7) break;
                    x = d;
                    result[x, y] = data[++i];
                    while (y != d)
                    {
                        result[++x, --y] = data[++i];
                    }
                    i++;
                }
                else
                {
                    result[x, y] = data[i];
                    if (i == 0)
                        x++;
                    while (x != 0)
                    {
                        i++;
                        x--;
                        y++;
                        result[x, y] = data[i];
                    }
                    while (y != 0)
                    {
                        i++;
                        if (flag || y == 1)
                            x++;
                        else
                            y += 2;
                        y--;
                        result[x, y] = data[i];
                        if (!flag)
                            flag = true;
                    }
                    x++;
                    y = 0;
                    i++;
                }
            } while (i < 64);

            result[7, 7] = data[63];

            return result;
        }
    }
}
