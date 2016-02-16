using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            saveToolStripMenuItem.Enabled = false;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP Files|*.bmp|JPG Files|*.jpg|PNG Files|*.png|RIPPEG Files|*.rippeg|All Files|*.*";
            DialogResult result = openFileDialog.ShowDialog(); // I want to open this to the child window in the file
            if (result == DialogResult.OK) // checks if the result returned true
            {
                string ext = Path.GetExtension(openFileDialog.FileName); // includes the period
                if(ext == ".rippeg")
                {
                    pictureBox2.Image = null;
                    openFile(openFileDialog.FileName);
                }
                else
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                    fileNameBox.Text = openFileDialog.FileName;
                    dataObj.setOriginal(new Bitmap(openFileDialog.FileName)); // sets the original bitmap to the loaded
                    dataObj.gHead.setHeight((short)dataObj.getOriginal().Height);
                    dataObj.gHead.setWidth((short)dataObj.getOriginal().Width);
                    dataObj.gHead.setQuality(1);
                    pictureBox2.Image = null;
                }
                rgbChangeButton.Enabled = true;
                ShowYButton.Enabled = false;
                showCbButton.Enabled = false;
                ShowCrButton.Enabled = false;
                showYCbCrButton.Enabled = false;
                // setup the header
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "RIPPEG Files|*.rippeg|All Files|*.*";
            DialogResult result = saveFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                this.saveFile(saveFileDialog.FileName);
            }
        }

        private void rgbChangeButton_Click(object sender, EventArgs e)
        {
            int padW = 0,
                padH = 0,
                sz = 0;
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
            int modH = dataObj.gHead.getHeight() % 8, 
                modW = dataObj.gHead.getWidth() % 8; // array is 1 # less for each
            if(modW != 0 || modH != 0)
            {
                padW = (8 - modW == 8) ? 0 : 8 - modW;
                padH = (8 - modH == 8) ? 0 : 8 - modH;
                dataObj.paddedWidth = dataObj.gHead.getWidth() + padW;
                dataObj.paddedHeight = dataObj.gHead.getHeight() + padH;
                dataObj.setyData(padData(dataObj.getyData(), padW, padH));
                dataObj.setCbData(padData(dataObj.getCbData(), padW, padH));
                dataObj.setCrData(padData(dataObj.getCrData(), padW, padH));
            }

            dataObj.finalData = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth * 3];
            dataObj.yEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.cbEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.crEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];

            int pos = 0;
            for (int y = 0; y < dataObj.paddedHeight; y += 8)
            {
                for (int x = 0; x < dataObj.paddedWidth; x += 8)
                {
                    sz += 64;
                    // (add 128 before)DCT, Quantize, ZigZag and RLE
                    // Y
                    tempY = generateBlocks(dataObj.getyData(), x, y);
                    tempDY = dctObj.forwardDCT(tempY);
                    // quantize
                    stempY = quantizeLuma(tempDY);
                    // zigzag
                    szztempY = zigzag(stempY);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.yEncoded, sz + 64);
                    Buffer.BlockCopy(szztempY, 0, dataObj.yEncoded, pos, 64);
                    // rle

                    // unrle

                    // unzigzag
                    stempY = unzigzag(szztempY);
                    // inverse quantize
                    tempDY = inverseQuantizeLuma(stempY);
                    tempY = dctObj.inverseDCTByte(tempDY);
                    putback(dataObj.getyData(), tempY, x, y);
                    
                    // Cb
                    tempCb = generateBlocks(dataObj.getCbData(), x, y);
                    tempDCb = dctObj.forwardDCT(tempCb);
                    // quantize
                    stempCb = quantizeData(tempDCb);
                    // zigzag
                    szztempB = zigzag(stempCb);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.cbEncoded, sz + 64);
                    Buffer.BlockCopy(szztempB, 0, dataObj.cbEncoded, pos, 64);
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

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.crEncoded, sz + 64);
                    Buffer.BlockCopy(szztempR, 0, dataObj.crEncoded, pos, 64);
                    // rle

                    // unrle

                    // unzigzag
                    stempCr = unzigzag(szztempR);
                    // inverse quantize
                    tempDCr = inverseQuantizeData(stempCr);
                    tempCr = dctObj.inverseDCTByte(tempDCr);
                    putback(dataObj.getCrData(), tempCr, x, y);
                    pos += 64;
                }
            }
            // update the RGBChanger data to what we have in the dataObj
            updateRGBChangerYCrCBData();
            setFinalData();

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
            saveToolStripMenuItem.Enabled = true;
            pictureBox2.Image = dataObj.getYCrCbtoRGB();
        }

        private void setFinalData()
        {
            int fd = 0;
            dataObj.finalData = new sbyte[dataObj.yEncoded.Length + dataObj.cbEncoded.Length + dataObj.crEncoded.Length];
            for(int i = 0; i < dataObj.yEncoded.Length; i++)
            {
                dataObj.finalData[fd++] = dataObj.yEncoded[i];
            }
            for (int jj = 0; jj < dataObj.cbEncoded.Length; jj++)
            {
                dataObj.finalData[fd++] = dataObj.cbEncoded[jj];
            }
            for (int kk = 0; kk < dataObj.crEncoded.Length; kk++)
            {
                dataObj.finalData[fd++] = dataObj.crEncoded[kk];
            }
        }

        // fix this
        private void splitFinalData()
        {
            int fd = 0;
            
            for (int i = 0; i < dataObj.yEncoded.Length; i++)
            {
                dataObj.yEncoded[i] = dataObj.finalData[fd++];
            }
            for (int jj = 0; jj < dataObj.cbEncoded.Length; jj++)
            {
                dataObj.cbEncoded[jj] = dataObj.finalData[fd++];
            }
            for (int kk = 0; kk < dataObj.crEncoded.Length; kk++)
            {
                dataObj.crEncoded[kk] = dataObj.finalData[fd++];
            }
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

        private sbyte[] generateBlocks(sbyte[] data, int offsetx, int offsety)
        {
            sbyte[] output = new sbyte[8 * 8];
            for (int i = 0; i < 64; i++)
            {
                output[i] = data[i + offsetx + offsety];
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

        private void putbacks(sbyte[,] original, sbyte[,] data, int offsetx, int offsety)
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

        public void saveFile(string fileName)
        {
            if (pictureBox2.Image == null) return;
            this.Text = fileName; // sets the text of the form to the file name
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter wr = new BinaryWriter(fs);
            // setup the header information
            writeData(wr, dataObj.gHead);
            writeData(wr, dataObj.finalData);
            wr.Close();
            fs.Close();
        }

        public void openFile(string fileName)
        {
            this.Text = fileName; // sets the text of the form to the file name
            BinaryReader re = new BinaryReader(File.OpenRead(fileName));
            // setup the header information
            readData(re, dataObj.gHead);
            dataObj.setRGBtoYCrCb(new Bitmap(dataObj.gHead.getWidth(), dataObj.gHead.getHeight()));
            int modH = dataObj.gHead.getHeight() % 8,
                modW = dataObj.gHead.getWidth() % 8, // array is 1 # less for each
                padW = 0,
                padH = 0;
            if (modW != 0 || modH != 0)
            {
                padW = (8 - modW == 8) ? 0 : 8 - modW;
                padH = (8 - modH == 8) ? 0 : 8 - modH;
                dataObj.paddedWidth = dataObj.gHead.getWidth() + padW;
                dataObj.paddedHeight = dataObj.gHead.getHeight() + padH;
            }
            dataObj.finalData = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth * 3];
            dataObj.yEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.cbEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.crEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.setyData(new byte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setCbData(new byte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setCrData(new byte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setsyData(new sbyte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setsCbData(new sbyte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setsCrData(new sbyte[dataObj.paddedWidth, dataObj.paddedHeight]);
            readData(re, dataObj.gHead, dataObj.finalData);
            // split the data
            splitFinalData();

            sbyte[] tempY, tempCb, tempCr;
            byte[,] btempY, btempCb, btempCr;
            sbyte[,] stempY, stempCb, stempCr;
            double[,] tempDY, tempDCb, tempDCr;
            for (int y = 0; y < dataObj.paddedHeight; y += 8)
            {
                for (int x = 0; x < dataObj.paddedWidth; x += 8)
                {
                    // DCT, Quantize, ZigZag and RLE
                    // Y
                    // block
                    tempY = generateBlocks(dataObj.yEncoded, x, y);
                    // unzigzag
                    stempY = unzigzag(tempY);
                    // inverse quantize
                    tempDY = inverseQuantizeLuma(stempY);
                    stempY = dctObj.sinverseDCTByte(tempDY);
                    putbacks(dataObj.getsyData(), stempY, x, y);

                    // Cb
                    // block
                    tempCb = generateBlocks(dataObj.cbEncoded, x, y);
                    // unzigzag
                    stempCb = unzigzag(tempCb);
                    // inverse quantize
                    tempDCb = inverseQuantizeData(stempCb);
                    stempCb = dctObj.sinverseDCTByte(tempDCb);
                    putbacks(dataObj.getsCbData(), stempCb, x, y);

                    // Cr
                    // block
                    tempCr = generateBlocks(dataObj.crEncoded, x, y);
                    // unzigzag
                    stempCr = unzigzag(tempCr);
                    // inverse quantize
                    tempDCr = inverseQuantizeData(stempCr);
                    stempCr = dctObj.sinverseDCTByte(tempDCr);
                    putbacks(dataObj.getsCrData(), stempCr, x, y);
                }
            }
            re.Close();
            // set pixels
            updateRGBChangerYCrCBData();

            dataObj.setYCrCbtoRGB(
                dataChanger.sYCbCrtoRGB(
                    dataObj.getRGBtoYCrCb()
                    ));
            updateRGBDataObject();
            dataChanger = new RGBChanger();

            ShowYButton.Enabled = true;
            showCbButton.Enabled = true;
            ShowCrButton.Enabled = true;
            showYCbCrButton.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            pictureBox2.Image = dataObj.getYCrCbtoRGB();
        }

        private void writeData(BinaryWriter file, Header header)
        {
            file.Write(header.getHeight());
            file.Write(header.getWidth());
            file.Write(header.getQuality());
        }

        private void writeData(BinaryWriter file, sbyte[] data)
        {
            for(int i = 0; i < dataObj.paddedHeight * dataObj.paddedWidth * 3; i++)
                file.Write(data[i]);
        }

        private void readData(BinaryReader file, Header header)
        {
            header.setHeight(file.ReadInt16());
            header.setWidth(file.ReadInt16());
            header.setQuality(file.ReadByte());
        }

        private void readData(BinaryReader file, Header head, sbyte[] data)
        {
            int i = 0;
            while(i < head.getHeight() * head.getWidth() * 3)
            {
                data[i++] = file.ReadSByte();
            }
        }
    }
}
