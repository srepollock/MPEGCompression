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
        byte[,] yData;
        byte[,] CbData;
        byte[,] CrData;
        byte[,] rData;
        byte[,] gData;
        byte[,] bData;
        Color[,] YCbCrData;

        private double[,] forwardDCTData;

        private byte[,] inverseDCTData;

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
        public void setrData(byte[,] data) { this.rData = data; }
        public void setgData(byte[,] data) { this.gData = data; }
        public void setbData(byte[,] data) { this.bData = data; }
        public void setYCrCbData(Color[,] data) { this.YCbCrData = data; }
    }
}
