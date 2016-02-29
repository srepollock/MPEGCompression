using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class RLE
    {
        // data will be saved as
            // (count, data)
        public static sbyte[] rle(sbyte[] data)
        {
            sbyte[] output = new sbyte[0];
            sbyte count = 1; // because we start at 1
            int pos = 0;
            sbyte s = data[0];
            for(int i = 1; i < data.Length; i++)
            {
                if(count == 127) // maximum size of sbyte
                {
                    // save to array
                    Array.Resize<sbyte>(ref output, output.Length + 2);
                    output[pos] = count;
                    output[pos + 1] = s;
                    pos += 2;
                    count = 1;
                    s = data[i];
                }
                if(s == data[i])
                {
                    count++;
                }
                else
                {
                    Array.Resize<sbyte>(ref output, output.Length + 2);
                    output[pos] = count;
                    output[pos + 1] = s;
                    s = data[i];
                    count = 1;
                    pos += 2;
                }
            }

            return output;
        }
        public static sbyte[] unrle(sbyte[] data)
        {
            sbyte[] output = new sbyte[0];
            sbyte count = 0; // because we start at 1
            sbyte s;
            int len;
            for (int i = 0; i < data.Length; i+=2)
            {
                count = data[i];
                s = data[i + 1];
                len = output.Length;
                Array.Resize<sbyte>(ref output, output.Length + count);
                for (; len < output.Length; len++)
                {
                    output[len] = s;
                }
            }
            return output;
        }
    }
}
