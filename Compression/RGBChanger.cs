using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compression
{
    class RGBChanger
    {

        byte[,] yData;
        byte[,] CbData;
        byte[,] CrData;
        byte[,] rData;
        byte[,] gData;
        byte[,] bData;
        Color[,] YCbCrData;

        /* Image data being passed in */
        public RGBChanger()
        {

        }

        /* 
            Takes in an array of data that is the image RGB data
            Then converts it to YCbCr data
            Then returns the YCbCr data

            Needs to take in the data of the image

            returns the new bitmap
        */
        public Bitmap RGBtoYCbCr(Bitmap orgBmp, Data dataObj)
        {
            Bitmap bmp = orgBmp;

            int width = dataObj.gHead.getWidth();
            int height = dataObj.gHead.getHeight();
            this.yData = new byte[width, height];                     //luma
            this.CbData = new byte[width, height];                     //Cb
            this.CrData = new byte[width, height];                     //Cr
            YCbCrData = new Color[width, height];

            Bitmap outBmp = new Bitmap(orgBmp.Width, orgBmp.Height);

            unsafe
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                int heightInPixels = bitmapData.Height;
                int widthInBytes = width * 3;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                //Convert to YCbCr
                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        int xPor3 = x * 3;
                        float blue = currentLine[xPor3++];
                        float green = currentLine[xPor3++];
                        float red = currentLine[xPor3];

                        yData[x, y] = (byte)(0 + (0.299 * red) + (0.587 * green) + (0.114 * blue));
                        CbData[x, y] = (byte)(128 - (0.168 * red) - (0.331264 * green) + (0.5 * blue));
                        CrData[x, y] = (byte)(128 + (0.5 * red) - (0.418688 * green) - (0.081312 * blue));

                        YCbCrData[x, y] = Color.FromArgb(yData[x, y], CbData[x, y], CrData[x, y]);
                    }
                }
                bmp.UnlockBits(bitmapData);
            }

            // I can use this image returned information's pixels to play around with

            Sampler.subsample(CbData, dataObj);
            Sampler.subsample(CrData, dataObj);

            // subsample
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    outBmp.SetPixel(x, y, YCbCrData[x, y]);
                }
            }

            return outBmp;
        }

        public Bitmap YCbCrtoRGB(Bitmap bmp)
        {

            int width = bmp.Width;
            int height = bmp.Height;
            this.rData = new byte[width, height];
            this.gData = new byte[width, height];
            this.bData = new byte[width, height];

            Bitmap outBmp = new Bitmap(bmp.Width, bmp.Height);

            // Can't use the height and width of original. Needs to be the sub sampled size
            // need to handle doubling up the information
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int r, g, b;

                    r = (int)(yData[x, y] + ( 1.402 * ((CrData[x, y] - 128))));
                    g = (int)(yData[x, y] - (0.34414 * (CbData[x,y] - 128)) - (0.71414 * (CrData[x,y] - 128)));
                    b = (int)(yData[x,y] + (1.772 * (CbData[x,y] - 128)));

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    rData[x, y] = (byte)r;
                    gData[x, y] = (byte)g;
                    bData[x, y] = (byte)b;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    outBmp.SetPixel(x, y, Color.FromArgb(rData[x, y], gData[x, y], bData[x, y]));
                }
            }

            return outBmp;
        }

        public Bitmap sYCbCrtoRGB(Data dataObj)
        {
            int width = dataObj.gHead.getWidth();
            int height = dataObj.gHead.getHeight();
            this.rData = new byte[width, height];
            this.gData = new byte[width, height];
            this.bData = new byte[width, height];

            Bitmap outBmp = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int r, g, b;

                    r = (int)(dataObj.getdyData()[x, y] + (1.402 * ((dataObj.getdCrData()[x, y] - 128))));
                    g = (int)(dataObj.getdyData()[x, y] - (0.34414 * (dataObj.getdCbData()[x, y] - 128)) - (0.71414 * (dataObj.getdCrData()[x, y] - 128)));
                    b = (int)(dataObj.getdyData()[x, y] + (1.772 * (dataObj.getdCbData()[x, y] - 128)));

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    rData[x, y] = (byte)r;
                    gData[x, y] = (byte)g;
                    bData[x, y] = (byte)b;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    outBmp.SetPixel(x, y, Color.FromArgb(rData[x, y], gData[x, y], bData[x, y]));
                }
            }

            return outBmp;
        }

        public byte[,] getyData() { return this.yData; }
        public byte[,] getCbData() { return this.CbData; }
        public byte[,] getCrData() { return this.CrData; }
        public byte[,] getrData() { return this.rData; }
        public byte[,] getgData() { return this.gData; }
        public byte[,] getbData() { return this.bData; }
        public Color[,] getYCrCbData() { return this.YCbCrData; }

        public void setyData(byte[,] data) { this.yData = data; }
        public void setCbData(byte[,] data) { this.CbData = data; }
        public void setCrData(byte[,] data) { this.CrData = data; }
    }
}
