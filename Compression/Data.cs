using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class Data
    {
        /*
            Original bitmap
        */
        private Bitmap original;
        /*
            Bitmap after RGB->YCrCb
        */
        private Bitmap RGBtoYCrCb;
        /*
            Bitmap after YCrCb->RGB
        */
        private Bitmap YCrCbtoRGB;
        
        public Data()
        {

        }

        public void setOriginal(Bitmap bmp)
        {
            this.original = bmp;
        }

        public Bitmap getOriginal()
        {
            return this.original;
        }

        public void setRGBtoYCrCb(Bitmap bmp)
        {
            this.RGBtoYCrCb = bmp;
        }

        public Bitmap getRGBtoYCrCb()
        {
            return this.RGBtoYCrCb;
        }

        public void setYCrCbtoRGB(Bitmap bmp)
        {
            this.YCrCbtoRGB = bmp;
        }

        public Bitmap getYCrCbtoRGB()
        {
            return this.YCrCbtoRGB;
        }
    }
}
