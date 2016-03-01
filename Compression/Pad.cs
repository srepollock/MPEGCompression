using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class Pad
    {
        public int modH, modW, padW, padH;
        public Pad(ref Data dataObj)
        {
            modH = dataObj.gHead.getHeight() % 16;
            modW = dataObj.gHead.getWidth() % 16; // array is 1 # less for each
            padW = 0;
            padH = 0;
            if (modW != 0 || modH != 0)
            {
                padW = (16 - modW == 16) ? 0 : 16 - modW;
                padH = (16 - modH == 16) ? 0 : 16 - modH;
                dataObj.paddedWidth = dataObj.gHead.getWidth() + padW;
                dataObj.paddedHeight = dataObj.gHead.getHeight() + padH;
            }
            else
            {
                dataObj.paddedWidth = dataObj.gHead.getWidth();
                dataObj.paddedHeight = dataObj.gHead.getHeight();
            }
        }
    }
}
