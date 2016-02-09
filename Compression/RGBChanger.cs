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
        // Do I need to pass in the data object?

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
            //Bitmap bmp = new Bitmap(orgBmp.Width, orgBmp.Height);
            Bitmap bmp = orgBmp;

            int width = bmp.Width;
            int height = bmp.Height;
            this.yData = new byte[width, height];                     //luma
            this.CbData = new byte[width, height];                     //Cb
            this.CrData = new byte[width, height];                     //Cr

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
                    }
                }
                bmp.UnlockBits(bitmapData);
            }


            Bitmap outBmp = new Bitmap(orgBmp.Width, orgBmp.Height);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    outBmp.SetPixel(x, y, Color.FromArgb(yData[x, y], CrData[x, y], CbData[x, y]));
                }
            }

            return outBmp;
        } // I need the arrays of the data Y, Cr, Cb

        public Bitmap YCbCrtoRGB(Bitmap bmp)
        {

            int width = bmp.Width;
            int height = bmp.Height;
            this.rData = new byte[width, height];
            this.gData = new byte[width, height];
            this.bData = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rData[x, y] = (byte)((1.164 * (yData[x,y] - 16)) + (0.0 * (CbData[x,y] - 128)) + (1.596 * (CrData[x,y] - 128)));
                    gData[x, y] = (byte)((1.164 * (yData[x,y] - 16)) + (-0.392 * (CbData[x,y] - 128)) + (-0.813 * (CrData[x,y] - 128)));
                    bData[x, y] = (byte)((1.164 * (yData[x, y] - 16)) + (2.017 * (CbData[x,y] - 128)) + (0.0 * (CrData[x,y] - 128)));
                }
            }

            Bitmap outBmp = new Bitmap(bmp.Width, bmp.Height);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
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
    }
}
