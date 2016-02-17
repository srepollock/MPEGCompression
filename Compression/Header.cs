using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    public class Header
    {
        int height, 
            width;
        byte quality;
        public Header()
        {

        }
        public Header(int h, int w, byte q)
        {
            this.height = h;
            this.width = w;
            this.quality = q;
        }
        // Getters
        public int getHeight() { return this.height; }
        public int getWidth() { return this.width; }
        public byte getQuality() { return this.quality; }
        // Setters
        public void setHeight(int h) { this.height = h; }
        public void setWidth(int w) { this.width = w; }
        public void setQuality(byte q) { this.quality = q; }
    }
}
