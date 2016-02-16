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
            zigzag();
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
            byte[,] tempCb, tempCr;
            double[,] tempDCb, tempDCr;
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
                    // Cb
                    tempCb = generateBlocks(dataObj.getCbData(), x, y);
                    tempDCb = dctObj.forwardDCT(tempCb);
                    // quantize
                    quantizeData(tempDCb);
                    // zigzag

                    // rle

                    // unrle

                    // unzigzag

                    // inverse quantize
                    inverseQuantizeData(tempDCb);
                    tempCb = dctObj.inverseDCTByte(tempDCb);
                    putback(dataObj.getCbData(), tempCb, x, y);
                    // Cr
                    tempCr = generateBlocks(dataObj.getCrData(), x, y);
                    tempDCr = dctObj.forwardDCT(tempCr);
                    // quantize
                    quantizeData(tempDCr);
                    // zigzag

                    // rle

                    // unrle

                    // unzigzag

                    // inverse quantize
                    inverseQuantizeData(tempDCr);
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
        private void quantizeData(double[,] data)
        {
            for(int y = 0; y < 8; y++)
            {
                for(int x = 0; x < 8; x++)
                {
                    data[x, y] = Math.Round(data[x, y] / dataObj.chrominance[x, y]);
                }
            }
        }

        private void inverseQuantizeData(double[,] data)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    data[x, y] = (data[x, y] * dataObj.chrominance[x, y]);
                }
            }
        }

        private void quantizeLuma(byte[,] data)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    data[x, y] = Convert.ToByte(Math.Round((double)(data[x, y] / dataObj.luminance[x, y])));
                }
            }
        }

        private void inverseQuantizeLuma(byte[,] data)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    data[x, y] = Convert.ToByte((data[x, y] * dataObj.luminance[x, y]));
                }
            }
        }

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

        //public byte[,] zigzag(byte[,] data)
        public byte[,] zigzag()
        {
            int n = 8;
            byte[,] result = new byte[n, n];
            int i = 0, j = 0;
            int d = -1; // -1 for top-right move, +1 for bottom-left move
            int start = 0, end = n * n - 1;

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

            do
            {
                //result[i, j] = start++;
                start++;
                result[i, j] = data[i, j];
                //result[n - i - 1, n - j - 1] = end--;
                result[n - i - 1, n - j - 1] = data[n - i - 1, n - j - 1];
                end--;

                i += d; j -= d;
                if (i < 0)
                {
                    i++; d = -d; // top reached, reverse
                }
                else if (j < 0)
                {
                    j++; d = -d; // left reached, reverse
                }
            } while (start < end);

            if (start == end)
                result[i, j] = data[i, j];
            return result;
        }
    }
}
