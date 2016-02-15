﻿using System;
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
            int height = dataObj.getOriginal().Height,
                width = dataObj.getOriginal().Width;
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
            int modH = height % 8, modW = width % 8; // array is 1 # less for each
            if(modW != 0 && modH != 0)
            {
                // both
                int padW = 8 - modW, 
                    padH = 8 - modH;
                // pad all 3 channels
            }
            else if(modW != 0)
            {
                // width
                int padW = 8 - modW;
            }
            else
            {
                // height
                int padh = 8 - modH;
            }

            //test dct and idct here
            for (int y = 0; y < height; y += 8)
            {
                for(int x = 0; x < width; x+= 8)
                {

                }
            }

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

        private void updateYCrCbDataObject()
        {
            dataObj.setyData(dataChanger.getyData());
            dataObj.setCbData(dataChanger.getCbData());
            dataObj.setCrData(dataChanger.getCrData());
            dataObj.setYCrCbData(dataChanger.getYCrCbData());
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
    }
}
