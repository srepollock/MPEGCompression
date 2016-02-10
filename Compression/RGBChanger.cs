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
        public Bitmap RGBtoYCbCr(Bitmap orgBmp)
        {
            Bitmap bmp = orgBmp;

            int width = bmp.Width;
            int height = bmp.Height;
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

                        yData[x, y] = (byte)(16 + (0.257 * red) + (0.504 * green) + (0.098 * blue));
                        CbData[x, y] = (byte)(128 + (-0.148 * red) + (-0.291 * green) + (0.438 * blue));
                        CrData[x, y] = (byte)(128 + (0.493 * red) + (-0.368 * green) + (-0.071 * blue));

                        YCbCrData[x, y] = Color.FromArgb(yData[x, y], CbData[x, y], CrData[x, y]);
                    }
                }
                bmp.UnlockBits(bitmapData);
            }

            // I can use this image returned information's pixels to play around with

            subsample(CbData, height, width);
            subsample(CrData, height, width);

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

                    r = (int)((1.164 * (yData[x, y] - 16)) + (0.0 * (CbData[x, y] - 128)) + (1.596 * (CrData[x, y] - 128)));
                    g = (int)((1.164 * (yData[x, y] - 16)) + (-0.392 * (CbData[x, y] - 128)) + (-0.813 * (CrData[x, y] - 128)));
                    b = (int)((1.164 * (yData[x, y] - 16)) + (2.017 * (CbData[x, y] - 128)) + (0.0 * (CrData[x, y] - 128)));

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

        private void subsample(byte[,] org, int height, int width)
        {
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 2)
                {
                    if ((y + 1) < height)
                        org[x + 1, y] = org[x + 1, y + 1] = org[x, y + 1] = org[x, y];
                    else
                        org[x + 1, y] = org[x, y];
                }
            }
        }
    }
}
