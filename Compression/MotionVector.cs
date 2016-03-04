using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class MotionVector
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public MotionVector()
        {
            
        }

        public MotionVector(int x, int y, int xx, int yy)
        {
            x1 = x;
            y1 = y;
            x2 = xx;
            y2 = yy;
        }
    }
}
