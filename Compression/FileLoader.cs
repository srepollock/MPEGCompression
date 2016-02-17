using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compression
{ 
    public partial class FileLoader : Form
    {
        /*
            This contains all the data for the conversion and such
        */
        Data dataObj;
        RGBChanger dataChanger;
        DCT dctObj;

        public FileLoader()
        {
            InitializeComponent();
            dataObj = new Data(); // sets up the data object to us methods
            dctObj = new DCT();
            dataChanger = new RGBChanger();
            rgbChangeButton.Enabled = false;
            ShowYButton.Enabled = false;
            showCbButton.Enabled = false;
            ShowCrButton.Enabled = false;
            showYCbCrButton.Enabled = false;
            /* //zigzag testing
            double[,] data = new double[,]
            {
                {0,1,5,6,14,15,27,28 },
                {2,4,7,13,16,26,29,42 },
                {3,8,12,17,25,30,41,43 },
                {9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };
            double[,] tempdata;
            double[] temp;
            temp = zigzag(data);
            tempdata = unzigzag(temp);
            if (tempdata.Length != 0)
                Console.WriteLine("ok");
            */
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP Files|*.bmp|JPG Files|*.jpg|PNG Files|*.png|All Files|*.*";
            DialogResult result = openFileDialog.ShowDialog(); // I want to open this to the child window in the file
            if (result == DialogResult.OK) // checks if the result returned true
            {
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                fileNameBox.Text = openFileDialog.FileName;
                dataObj.setOriginal(new Bitmap(openFileDialog.FileName)); // sets the original bitmap to the loaded
                rgbChangeButton.Enabled = true;
                ShowYButton.Enabled = false;
                showCbButton.Enabled = false;
                ShowCrButton.Enabled = false;
                showYCbCrButton.Enabled = false;
            }
        }

        private void rgbChangeButton_Click(object sender, EventArgs e)
        {
            int orgheight = dataObj.getOriginal().Height,
                orgwidth = dataObj.getOriginal().Width;
            int padW = 0,
                padH = 0,
                width = orgwidth,
                height = orgheight;
            byte[,] tempY, tempCb, tempCr;
            sbyte[,] stempY, stempCb, stempCr;
            double[,] tempDY, tempDCb, tempDCr;
            sbyte[] szztempY, szztempB, szztempR;
            /* This data needs to be saved for the header information */

            dataObj.setRGBtoYCrCb(
                dataChanger.RGBtoYCbCr(
                    dataObj.getOriginal()
                    )); // This will set the data changed bitmap to that of the returned bitmap from the data changer
            updateYCrCbDataObject();

            // This will be run on Cb and Cr data
            // run dct and quantize here. We pass in 8x8's
            // update offset by incrementing by 8 each time
            // generate 8x8 blocks

            // check if the size is divisible by 8, if not pad
            int modH = orgheight % 8, modW = orgwidth % 8; // array is 1 # less for each
            if(modW != 0 || modH != 0)
            {
                padW = (8 - modW == 8) ? 0 : 8 - modW;
                padH = (8 - modH == 8) ? 0 : 8 - modH;
                width = orgwidth + padW;
                height = orgheight + padH;
                dataObj.setyData(padData(dataObj.getyData(), padW, padH));
                dataObj.setCbData(padData(dataObj.getCbData(), padW, padH));
                dataObj.setCrData(padData(dataObj.getCrData(), padW, padH));
            }

            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x += 8)
                {
                    // (add 128 before)DCT, Quantize, ZigZag and RLE
                    // Y
                    tempY = generateBlocks(dataObj.getyData(), x, y);
                    tempDY = dctObj.forwardDCT(tempY);
                    // quantize
                    stempY = quantizeLuma(tempDY);
                    // zigzag
                    szztempY = zigzag(stempY);
                    // rle

                    // unrle

                    // unzigzag
                    stempY = unzigzag(szztempY);
                    // inverse quantize
                    inverseQuantizeLuma(stempY);
                    tempY = dctObj.inverseDCTByte(tempDY);
                    putback(dataObj.getyData(), tempY, x, y);
                    
                    // Cb
                    tempCb = generateBlocks(dataObj.getCbData(), x, y);
                    tempDCb = dctObj.forwardDCT(tempCb);
                    // quantize
                    stempCb = quantizeData(tempDCb);
                    // zigzag
                    szztempB = zigzag(stempCb);
                    // rle

                    // unrle

                    // unzigzag
                    stempCb = unzigzag(szztempB);
                    // inverse quantize
                    tempDCb = inverseQuantizeData(stempCb);
                    tempCb = dctObj.inverseDCTByte(tempDCb);
                    putback(dataObj.getCbData(), tempCb, x, y);
                    
                    // Cr
                    tempCr = generateBlocks(dataObj.getCrData(), x, y);
                    tempDCr = dctObj.forwardDCT(tempCr);
                    // quantize
                    stempCr = quantizeData(tempDCr);
                    // zigzag
                    szztempR = zigzag(stempCr);
                    // rle

                    // unrle

                    // unzigzag
                    stempCr = unzigzag(szztempR);
                    // inverse quantize
                    tempDCr = inverseQuantizeData(stempCr);
                    tempCr = dctObj.inverseDCTByte(tempDCr);
                    putback(dataObj.getCrData(), tempCr, x, y);
                }
            }
            // update the RGBChanger data to what we have in the dataObj
            updateRGBChangerYCrCBData();

            dataObj.setYCrCbtoRGB(
                dataChanger.YCbCrtoRGB(
                    dataObj.getRGBtoYCrCb()
                    ));
            updateRGBDataObject();
            dataChanger = new RGBChanger();

            ShowYButton.Enabled = true;
            showCbButton.Enabled = true;
            ShowCrButton.Enabled = true;
            showYCbCrButton.Enabled = true;
            pictureBox2.Image = dataObj.getYCrCbtoRGB();
        }

        private byte[,] padData(byte[,] data, int padxby, int padyby)
        {
            int width = dataObj.getOriginal().Width, height = dataObj.getOriginal().Height;
            byte[,] temp = new byte[width + padxby, height + padyby];
            for (int y = 0; y < height + padyby; y++)
            {
                for(int x = 0; x < width + padxby; x++)
                {
                    if(x >= width && y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else if (x >= width)
                    {
                        temp[x, y] = 0;
                    }
                    else if (y >= height)
                    {
                        temp[x, y] = 0;
                    }
                    else
                    {
                        temp[x, y] = data[x, y];
                    }
                }
            }
            return temp;
        }

        private void updateYCrCbDataObject()
        {
            dataObj.setyData(dataChanger.getyData());
            dataObj.setCbData(dataChanger.getCbData());
            dataObj.setCrData(dataChanger.getCrData());
            dataObj.setYCrCbData(dataChanger.getYCrCbData());
        }

        private void updateRGBChangerYCrCBData()
        {
            dataChanger.setyData(dataObj.getyData());
            dataChanger.setCbData(dataObj.getCbData());
            dataChanger.setCrData(dataObj.getCrData());
        }

        private void updateRGBDataObject()
        {
            dataObj.setrData(dataChanger.getrData());
            dataObj.setgData(dataChanger.getgData());
            dataObj.setbData(dataChanger.getbData());
        }

        private byte[,] generateBlocks(byte[,] data, int offsetx, int offsety)
        {
            byte[,] output = new byte[8, 8];
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = data[offsetx + x, offsety + y];
                }
            }
            return output;
        }

        private void putback(byte[,] original, byte[,] data, int offsetx, int offsety)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    original[offsetx + x, offsety + y] = data[x, y];
                }
            }
        }

        private void ShowYButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getYBitmap(pictureBox1.Image);
        }

        private void ShowCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCrBitmap(pictureBox1.Image);
        }

        private void showCbButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCbBitmap(pictureBox1.Image);
        }

        private void showYCbCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getRGBtoYCrCb();
        }

        // Need the data and the quantizaion table (public from data class)
        private sbyte[,] quantizeData(double[,] data)
        {
            sbyte[,] output = new sbyte[8,8];
            for(int y = 0; y < 8; y++)
            {
                for(int x = 0; x < 8; x++)
                {
                    output[x, y] = Convert.ToSByte(Math.Round(data[x, y] / dataObj.chrominance[x, y]));
                }
            }
            return output;
        }

        private double[,] inverseQuantizeData(sbyte[,] data)
        {
            double[,] output = new double[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = (data[x, y] * dataObj.chrominance[x, y]);
                }
            }
            return output;
        }

        private sbyte[,] quantizeLuma(double[,] data)
        {
            sbyte[,] output = new sbyte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = Convert.ToSByte(Math.Round((double)(data[x, y] / dataObj.luminance[x, y])));
                }
            }
            return output;
        }

        private double[,] inverseQuantizeLuma(sbyte[,] data)
        {
            double[,] output = new double[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[x, y] = data[x, y] * dataObj.luminance[x, y];
                }
            }
            return output;
        }

        private sbyte[] zigzag(sbyte[,] data)
        {
            // testing
            /*
            byte[,] data = new byte[,]
            {
                {0,1,5,6,14,15,27,28 },
                {2,4,7,13,16,26,29,42 },
                {3,8,12,17,25,30,41,43 },
                {9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };
            */
            sbyte[] result = new sbyte[8 * 8];

            int i = 0,
                x = 0,
                y = 0,
                d = 0; // -1 for the to-right move, +1 for the bottom-left move
            bool flag = false;
            bool reverseFlag = false;

            do
            {
                flag = false;
                if (x > 7 || reverseFlag)
                {
                    d++;
                    y = d;
                    reverseFlag = true;
                    if(x > 7) x--;
                    result[i] = data[x, y];
                    while (x != d)
                    {
                        result[++i] = data[--x, ++y];
                    }
                    if(++d > 7) break;
                    x = d;
                    result[++i] = data[x, y];
                    while (y != d)
                    {
                        result[++i] = data[++x, --y];
                    }
                    i++;
                }
                else
                {
                    result[i] = data[x, y];
                    if (i == 0) x++;
                    while (x != 0)
                    {
                        result[++i] = data[--x, ++y];
                    }
                    while (y != 0)
                    {
                        if (flag || y == 1) x++;
                        else  y += 2;
                        result[++i] = data[x, --y];
                        if (!flag) flag = true;
                    }
                    x++; y = 0; i++;
                }
            } while (i < 64);

            result[63] = data[7, 7];

            return result;
        }

        private sbyte[,] unzigzag(sbyte[] data)
        {
            // testing
            /*
            byte[,] data = new byte[,]
            {
                {0,1,5,6,14,15,27,28 },
                {2,4,7,13,16,26,29,42 },
                {3,8,12,17,25,30,41,43 },
                {9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };
            */
            sbyte[,] result = new sbyte[8, 8];

            int i = 0,
                x = 0,
                y = 0,
                d = 0; // -1 for the to-right move, +1 for the bottom-left move
            bool flag = false;
            bool reverseFlag = false;

            do
            {
                flag = false;
                if (x >= 8 || reverseFlag)
                {
                    d++;
                    y = d;
                    reverseFlag = true;
                    if (x >= 8)
                        x--;
                    result[x,y] = data[i];
                    while (x != d)
                    {
                        result[--x, ++y] = data[++i];
                    }
                    if (++d > 7) break;
                    x = d;
                    result[x, y] = data[++i];
                    while (y != d)
                    {
                        result[++x, --y] = data[++i];
                    }
                    i++;
                }
                else
                {
                    result[x, y] = data[i];
                    if (i == 0)
                        x++;
                    while (x != 0)
                    {
                        i++;
                        x--;
                        y++;
                        result[x, y] = data[i];
                    }
                    while (y != 0)
                    {
                        i++;
                        if (flag || y == 1)
                            x++;
                        else
                            y += 2;
                        y--;
                        result[x, y] = data[i];
                        if (!flag)
                            flag = true;
                    }
                    x++;
                    y = 0;
                    i++;
                }
            } while (i < 64);

            result[7, 7] = data[63];

            return result;
        }
    }
}
