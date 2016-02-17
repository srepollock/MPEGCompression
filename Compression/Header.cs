using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class Header
    {
        short height, 
              width;
        byte quality;
        public Header()
        {

        }
        public Header(short h, short w, byte q)
        {
            this.height = h;
            this.width = w;
            this.quality = q;
        }
        // Getters
        public short getHeight() { return this.height; }
        public short getWidth() { return this.width; }
        public byte getQuality() { return this.quality; }
        // Setters
        public void setHeight(short h) { this.height = h; }
        public void setWidth(short w) { this.width = w; }
        public void setQuality(byte q) { this.quality = q; }
    }
}
