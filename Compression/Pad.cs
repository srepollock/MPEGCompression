using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    /// <summary>
    /// Pad class
    /// This class is to get the size of the padding to either pad or read
    /// the padded data by.
    /// Redundent, but cleans up the code. May just put back into the
    /// fileloader class later.
    /// </summary>
    class Pad
    {
        /// <summary>
        /// Public modified height, widht, padded heght, width
        /// </summary>
        public int modH, modW, padW, padH;
        /// <summary>
        /// Takes in a reference of the data object to read in the
        /// header height and widht of the original image, and sets
        /// the padded height and width of the data object for
        /// later use.
        /// </summary>
        /// <param name="dataObj">Data object to set the padded data and get the header</param>
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
