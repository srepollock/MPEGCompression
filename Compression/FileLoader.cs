﻿using System;
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
        ZigZag zz;
        Blocks block;
        Quantize q;

        public FileLoader()
        {
            InitializeComponent();
            dataObj = new Data(); // sets up the data object to us methods
            dctObj = new DCT();
            dataChanger = new RGBChanger();
            zz = new ZigZag();
            block = new Blocks();
            q = new Quantize();
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
            int sz = 0;
            byte[,] tempY, tempCb, tempCr;
            sbyte[,] stempY, stempCb, stempCr;
            double[,] tempDY, tempDCb, tempDCr;
            sbyte[] szztempY, szztempB, szztempR;
            /* This data needs to be saved for the header information */

            Pad padding = new Pad(ref dataObj);

            dataChanger.RGBtoYCbCr(
                dataObj.getOriginal(), ref dataObj
                ); // This will set the data changed bitmap to that of the returned bitmap from the data changer

            // pad data
            dataObj.setyData(padData(dataObj.getyData(), padding.padW, padding.padH));
            dataObj.setCbData(padData(dataObj.getCbData(), padding.padW, padding.padH));
            dataObj.setCrData(padData(dataObj.getCrData(), padding.padW, padding.padH));

            dataObj.finalData = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth * 3];
            dataObj.yEncoded = new sbyte[dataObj.paddedHeight * dataObj.paddedWidth];
            dataObj.cbEncoded = new sbyte[(dataObj.paddedHeight / 2) * (dataObj.paddedWidth / 2)];
            dataObj.crEncoded = new sbyte[(dataObj.paddedHeight / 2) * (dataObj.paddedWidth / 2)];

            int pos = 0;
            for (int y = 0; y < dataObj.paddedHeight; y += 8)
            {
                for (int x = 0; x < dataObj.paddedWidth; x += 8)
                {
                    sz += 64;
                    // (add 128 before)DCT, Quantize, ZigZag and RLE
                    // Y
                    tempY = block.generate2DBlocks(dataObj.getyData(), x, y);
                    tempDY = dctObj.forwardDCT(tempY);
                    // quantize
                    stempY = q.quantizeLuma(tempDY, dataObj);
                    // zigzag
                    szztempY = zz.zigzag(stempY);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.yEncoded, sz);
                    Buffer.BlockCopy(szztempY, 0, dataObj.yEncoded, pos, 64);
                    // rle

                    // unrle

                    // unzigzag
                    stempY = zz.unzigzag(szztempY);
                    // inverse quantize
                    tempDY = q.inverseQuantizeLuma(stempY, dataObj);
                    tempY = dctObj.inverseDCTByte(tempDY);
                    block.putback(dataObj.getyData(), tempY, x, y);
                    pos += 64;
                }
            }
            dataObj.setCbData(Sampler.subsample(dataObj.CbData, ref dataObj));
            dataObj.setCrData(Sampler.subsample(dataObj.CrData, ref dataObj));
            pos = 0;
            sz = 0;
            for (int j = 0; j < dataObj.paddedHeight / 2; j += 8)
            {
                for (int i = 0; i < dataObj.paddedWidth / 2; i += 8)
                {
                    sz += 64;
                    // Cb (data is subsampled)
                    tempCb = block.generate2DBlocks(dataObj.getCbData(), i, j);
                    tempDCb = dctObj.forwardDCT(tempCb);
                    // quantize
                    stempCb = q.quantizeData(tempDCb, dataObj);
                    // zigzag
                    szztempB = zz.zigzag(stempCb);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.cbEncoded, sz);
                    Buffer.BlockCopy(szztempB, 0, dataObj.cbEncoded, pos, 64);
                    // rle

                    // unrle

                    // unzigzag
                    stempCb = zz.unzigzag(szztempB);
                    // inverse quantize
                    tempDCb = q.inverseQuantizeData(stempCb, dataObj);
                    tempCb = dctObj.inverseDCTByte(tempDCb);
                    block.putback(dataObj.getCbData(), tempCb, i, j);

                    // Cr (data is subsampled)
                    tempCr = block.generate2DBlocks(dataObj.getCrData(), i, j);
                    tempDCr = dctObj.forwardDCT(tempCr);
                    // quantize
                    stempCr = q.quantizeData(tempDCr, dataObj);
                    // zigzag
                    szztempR = zz.zigzag(stempCr);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.crEncoded, sz);
                    Buffer.BlockCopy(szztempR, 0, dataObj.crEncoded, pos, 64);
                    // rle

                    // unrle

                    // unzigzag
                    stempCr = zz.unzigzag(szztempR);
                    // inverse quantize
                    tempDCr = q.inverseQuantizeData(stempCr, dataObj);
                    tempCr = dctObj.inverseDCTByte(tempDCr);
                    block.putback(dataObj.getCrData(), tempCr, i, j);
                    pos += 64;
                }
            }
            // rle the data
            dataObj.yEncoded = RLE.rle(dataObj.yEncoded);
            dataObj.cbEncoded = RLE.rle(dataObj.cbEncoded);
            dataObj.crEncoded = RLE.rle(dataObj.crEncoded);
            // set the header information
            dataObj.gHead.setYlen(dataObj.yEncoded.Length);
            dataObj.gHead.setCblen(dataObj.cbEncoded.Length);
            dataObj.gHead.setCrlen(dataObj.crEncoded.Length);
            // update the RGBChanger data to what we have in the dataObj
            setFinalData();

            // upsample data
            dataObj.setCbData(Sampler.upsample(dataObj.getCbData(), ref dataObj));
            dataObj.setCrData(Sampler.upsample(dataObj.getCrData(), ref dataObj));

            dataChanger.YCbCrtoRGB(ref dataObj);
            dataChanger = new RGBChanger();

            ShowYButton.Enabled = true;
            showCbButton.Enabled = true;
            ShowCrButton.Enabled = true;
            showYCbCrButton.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            pictureBox2.Image = dataObj.generateBitmap();
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
            int width = dataObj.gHead.getWidth(), height = dataObj.gHead.getHeight();
            byte[,] temp = new byte[width + padxby, height + padyby];
            for (int y = 0; y < height + padyby; y++)
            {
                for (int x = 0; x < width + padxby; x++)
                {
                    if (x >= width && y >= height)
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

        private byte[,] cpadData(byte[,] data, int padxby, int padyby)
        {
            int width = dataObj.gHead.getWidth(), height = dataObj.gHead.getHeight();
            byte[,] temp = new byte[(width + padxby) / 2, (height + padyby) / 2];
            for (int y = 0; y < (height + padyby) / 2; y++)
            {
                for (int x = 0; x < (width + padxby) / 2; x++)
                {
                    if (x >= width / 2 && y >= height / 2)
                    {
                        temp[x, y] = 0;
                    }
                    else if (x >= width / 2)
                    {
                        temp[x, y] = 0;
                    }
                    else if (y >= height / 2)
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
        /*
        private void updateYCrCbDataObject()
        {
            dataObj.setyData(dataChanger.getyData());
            // subsample data
            dataObj.setCbData(Sampler.subsample(dataChanger.getCbData(), dataObj));
            dataObj.setCrData(Sampler.subsample(dataChanger.getCrData(), dataObj));

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
        */
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

        public void saveFile(string fileName)
        {
            if (pictureBox2.Image == null) return;
            this.Text = fileName; // sets the text of the form to the file name
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter wr = new BinaryWriter(fs);
            // setup the header information
                // height, width, ylen, cblen, crlen, quality
            writeData(wr, dataObj.gHead);
            writeData(wr, dataObj.gHead, dataObj.finalData);
            wr.Close();
            fs.Close();
        }

        public void openFile(string fileName)
        {
            this.Text = fileName; // sets the text of the form to the file name
            BinaryReader re = new BinaryReader(File.OpenRead(fileName));
            // setup the header information
            readData(re, dataObj.gHead);

            Pad padding = new Pad(ref dataObj);

            dataObj.finalData = new sbyte[dataObj.gHead.getYlen() + dataObj.gHead.getCblen() + dataObj.gHead.getCrlen()];
            dataObj.yEncoded = new sbyte[dataObj.gHead.getYlen()];
            dataObj.cbEncoded = new sbyte[dataObj.gHead.getCblen()];
            dataObj.crEncoded = new sbyte[dataObj.gHead.getCrlen()];
            dataObj.setyData(new byte[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setCbData(new byte[dataObj.paddedWidth / 2, dataObj.paddedHeight / 2]);
            dataObj.setCrData(new byte[dataObj.paddedWidth / 2, dataObj.paddedHeight / 2]);
            dataObj.setdyData(new double[dataObj.paddedWidth, dataObj.paddedHeight]);
            dataObj.setdCbData(new double[dataObj.paddedWidth / 2, dataObj.paddedHeight / 2]);
            dataObj.setdCrData(new double[dataObj.paddedWidth / 2, dataObj.paddedHeight / 2]);
            // read the data
            readData(re, dataObj.gHead, dataObj.finalData);
            // split the data
            splitFinalData();
            // unrle the data
            dataObj.yEncoded = RLE.unrle(dataObj.yEncoded);
            dataObj.cbEncoded = RLE.unrle(dataObj.cbEncoded);
            dataObj.crEncoded = RLE.unrle(dataObj.crEncoded);


            sbyte[] tempY, tempCb, tempCr;
            sbyte[,] stempY, stempCb, stempCr;
            double[,] tempDY, tempDCb, tempDCr;
            int pos = 0;
            for (int y = 0; y < dataObj.paddedHeight; y += 8)
            {
                for (int x = 0; x < dataObj.paddedWidth; x += 8)
                {
                    // DCT, Quantize, ZigZag and RLE
                    // Y
                    // block
                    tempY = block.generateBlocks(dataObj.yEncoded, pos); // put in x, y here for cool spirals
                    // unzigzag
                    stempY = zz.unzigzag(tempY);
                    // inverse quantize
                    tempDY = q.inverseQuantizeLuma(stempY, dataObj);
                    tempDY = dctObj.dinverseDCT(tempDY);
                    block.putbackd(dataObj.getdyData(), tempDY, x, y);
                    pos += 64;
                }
            }
            pos = 0;
            for (int y = 0; y < dataObj.paddedHeight / 2; y += 8)
            {
                for (int x = 0; x < dataObj.paddedWidth / 2; x += 8)
                {
                    // Cb
                    // block
                    tempCb = block.generateBlocks(dataObj.cbEncoded, pos);
                    // unzigzag
                    stempCb = zz.unzigzag(tempCb);
                    // inverse quantize
                    tempDCb = q.inverseQuantizeData(stempCb, dataObj);
                    tempDCb = dctObj.dinverseDCT(tempDCb);
                    block.putbackd(dataObj.getdCbData(), tempDCb, x, y);

                    // Cr
                    // block
                    tempCr = block.generateBlocks(dataObj.crEncoded, pos);
                    // unzigzag
                    stempCr = zz.unzigzag(tempCr);
                    // inverse quantize
                    tempDCr = q.inverseQuantizeData(stempCr, dataObj);
                    tempDCr = dctObj.dinverseDCT(tempDCr);
                    block.putbackd(dataObj.getdCrData(), tempDCr, x, y);
                    pos += 64;
                }
            }
            re.Close();
            // set pixels

            dataObj.setdCbData(Sampler.upsample(dataObj.dCbData, ref dataObj));
            dataObj.setdCrData(Sampler.upsample(dataObj.dCrData, ref dataObj));

            dataChanger.sYCbCrtoRGB(
                ref dataObj
                );
            dataChanger = new RGBChanger();

            ShowYButton.Enabled = true;
            showCbButton.Enabled = true;
            ShowCrButton.Enabled = true;
            showYCbCrButton.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            pictureBox2.Image = dataObj.generateBitmap();
        }

        private void writeData(BinaryWriter file, Header header)
        {
            file.Write(header.getHeight());
            file.Write(header.getWidth());
            file.Write(header.getYlen());
            file.Write(header.getCblen());
            file.Write(header.getCrlen());
            file.Write(header.getQuality());
        }

        private void writeData(BinaryWriter file, Header header, sbyte[] data)
        {
            int c = 0;
            for (int i = 0; i < header.getYlen(); i++)
                file.Write(data[c++]);
            for (int i = 0; i < header.getCblen(); i++)
                file.Write(data[c++]);
            for (int i = 0; i < header.getCrlen(); i++)
                file.Write(data[c++]);
        }

        private void readData(BinaryReader file, Header header)
        {
            header.setHeight(file.ReadInt16());
            header.setWidth(file.ReadInt16());
            header.setYlen(file.ReadInt32());
            header.setCblen(file.ReadInt32());
            header.setCrlen(file.ReadInt32());
            header.setQuality(file.ReadByte());
        }

        private void readData(BinaryReader file, Header header, sbyte[] data)
        {
            
            Pad p = new Pad(ref dataObj);

            int c = 0;
            for(int i = 0; i < header.getYlen(); i++)
            {
                data[c++] = file.ReadSByte();
            }
            for (int j = 0; j < header.getCblen(); j++)
            {
                data[c++] = file.ReadSByte();
            }
            for (int k = 0; k < header.getCrlen(); k++)
            {
                data[c++] = file.ReadSByte();
            }
        }
    }
}
