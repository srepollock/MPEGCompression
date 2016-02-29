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
        int ylen,
            cblen,
            crlen;
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
        public int getYlen() { return this.ylen; }
        public int getCblen() { return this.cblen; }
        public int getCrlen() { return this.crlen; }
        public byte getQuality() { return this.quality; }
        // Setters
        public void setHeight(short h) { this.height = h; }
        public void setWidth(short w) { this.width = w; }
        public void setYlen(int y) { this.ylen = y; }
        public void setCblen(int cb) { this.cblen = cb; }
        public void setCrlen(int cr) { this.crlen = cr; }
        public void setQuality(byte q) { this.quality = q; }
    }
}
