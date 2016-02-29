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

        /*
            Image byte data
        */
        public byte[,] yData;
        public byte[,] CbData;
        public byte[,] CrData;
        public double[,] dyData;
        public double[,] dCbData;
        public double[,] dCrData;
        byte[,] rData;
        byte[,] gData;
        byte[,] bData;
        Color[,] YCbCrData;
        double[,] forwardDCTData;
        byte[,] inverseDCTData;

        public int orgWidth, 
                   orgHeight, 
                   paddedWidth, 
                   paddedHeight;

        public sbyte[] finalData;
        public sbyte[] yEncoded,
                       cbEncoded,
                       crEncoded;

        public Header gHead = new Header();

        /*
            Quantization Tables
        */
        public readonly int[,] luminance = {
            { 16, 11, 10, 16, 24, 40, 51, 61 },
            { 12, 12, 14, 19, 26, 58, 60, 55 },
            { 14, 13, 16, 24, 40, 57, 69, 56 },
            { 14, 17, 22, 29, 51, 87, 80, 62 },
            { 18, 22, 37, 56, 68, 109, 103, 77 },
            { 24, 35, 55, 64, 81, 104, 113, 92 },
            { 49, 64, 78, 87, 103, 121, 120, 101 },
            { 72, 92, 95, 98, 112, 100, 103, 99 }
        };

        public readonly int[,] chrominance = {
            { 17, 18, 24, 27, 47, 99, 99, 99 },
            { 18, 21, 26, 66, 99, 99, 99, 99 },
            { 24, 26, 56, 99, 99, 99, 99, 99 },
            { 47, 66, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 }
        };

        public Data()
        {

        }

        public Bitmap getYBitmap(Image org)
        {
            Bitmap outBmp = new Bitmap(org.Width, org.Height);
            for (int y = 0; y < org.Height; y++)
            {
                for (int x = 0; x < org.Width; x++)
                {
                    //outBmp.SetPixel(x, y, Color.FromArgb(yData[x,y]/3, yData[x, y] / 3, yData[x, y] / 3));
                    outBmp.SetPixel(x, y, Color.FromArgb(yData[x, y], yData[x, y], yData[x, y]));
                }
            }

            return outBmp;
        }

        public Bitmap getCrBitmap(Image org)
        {
            Bitmap outBmp = new Bitmap(org.Width, org.Height);
            for (int y = 0; y < org.Height; y++)
            {
                for (int x = 0; x < org.Width; x++)
                {
                    //outBmp.SetPixel(x, y, Color.FromArgb(CrData[x, y] / 3, CrData[x, y] / 3, CrData[x, y] / 3));
                    outBmp.SetPixel(x, y, Color.FromArgb(CrData[x, y], CrData[x, y], CrData[x, y]));
                }
            }

            return outBmp;
        }

        public Bitmap getCbBitmap(Image org)
        {
            Bitmap outBmp = new Bitmap(org.Width, org.Height);
            for (int y = 0; y < org.Height; y++)
            {
                for (int x = 0; x < org.Width; x++)
                {
                    //outBmp.SetPixel(x, y, Color.FromArgb(CbData[x, y] / 3, CbData[x, y] / 3, CbData[x, y] / 3));
                    outBmp.SetPixel(x, y, Color.FromArgb(CbData[x, y], CbData[x, y], CbData[x, y]));
                }
            }

            return outBmp;
        }

        public Bitmap getYCbCrBitmap(Image org)
        {
            Bitmap outBmp = new Bitmap(org.Width, org.Height);
            for (int y = 0; y < org.Height; y++)
            {
                for (int x = 0; x < org.Width; x++)
                {
                    //outBmp.SetPixel(x, y, Color.FromArgb(CbData[x, y] / 3, CbData[x, y] / 3, CbData[x, y] / 3));
                    outBmp.SetPixel(x, y, YCbCrData[x, y]);
                }
            }

            return outBmp;
        }

        public Bitmap generateBitmap() {
            Bitmap outBmp = new Bitmap(gHead.getWidth(), gHead.getHeight()); 
            for (int y = 0; y < gHead.getHeight(); y++) 
            { 
                for (int x = 0; x < gHead.getWidth(); x++) 
                { 
                    outBmp.SetPixel(x, y, Color.FromArgb(rData[x,y], gData[x,y], bData[x,y])); 
                }
            }
            return outBmp;
        }

        /*
            Getters
        */
        public Bitmap getOriginal() { return this.original; }
        public Bitmap getRGBtoYCrCb() { return this.RGBtoYCrCb; }
        public Bitmap getYCrCbtoRGB() { return this.YCrCbtoRGB; }
        public double[,] getForwardDCTData() { return this.forwardDCTData; }
        //public double[,] getInverseDCTData() { return this.inverseDCTData; }
        //testing
        public byte[,] getInverseDCTData() { return this.inverseDCTData; }
        public byte[,] getyData() { return this.yData; }
        public byte[,] getCbData() { return this.CbData; }
        public byte[,] getCrData() { return this.CrData; }
        public double[,] getdyData() { return this.dyData; }
        public double[,] getdCbData() { return this.dCbData; }
        public double[,] getdCrData() { return this.dCrData; }
        public byte[,] getrData() { return this.rData; }
        public byte[,] getgData() { return this.gData; }
        public byte[,] getbData() { return this.bData; }
        public Color[,] getYCrCbData() { return this.YCbCrData; }

        /*
            Setters
        */
        public void setOriginal(Bitmap bmp) { this.original = bmp; }
        public void setRGBtoYCrCb(Bitmap bmp) { this.RGBtoYCrCb = bmp; }
        public void setYCrCbtoRGB(Bitmap bmp) { this.YCrCbtoRGB = bmp; }
        public void setForwardDCTData(double[,] data) { this.forwardDCTData = data; }
        //public void setInverseDCTData(double[,] data) { this.inverseDCTData = data; }
        //testing
        public void setInverseDCTData(byte[,] data) { this.inverseDCTData = data; }
        public void setyData(byte[,] data) { this.yData = data; }
        public void setCbData(byte[,] data) { this.CbData = data; }
        public void setCrData(byte[,] data) { this.CrData = data; }
        public void setdyData(double[,] data) { this.dyData = data; }
        public void setdCbData(double[,] data) { this.dCbData = data; }
        public void setdCrData(double[,] data) { this.dCrData = data; }
        public void setrData(byte[,] data) { this.rData = data; }
        public void setgData(byte[,] data) { this.gData = data; }
        public void setbData(byte[,] data) { this.bData = data; }
        public void setYCrCbData(Color[,] data) { this.YCbCrData = data; }
    }
}
