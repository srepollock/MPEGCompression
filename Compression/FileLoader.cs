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
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPG Files|*.jpg|PNG Files|*.png|BMP Files|*.bmp|All Files|*.*";
            DialogResult result = openFileDialog.ShowDialog(); // I want to open this to the child window in the file
            if (result == DialogResult.OK) // checks if the result returned true
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                //pictureBox1.Image = new Bitmap(openFileDialog.FileName);
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
            dataObj.setRGBtoYCrCb(
                dataChanger.RGBtoYCbCr(
                    dataObj.getOriginal()
                    )); // This will set the data changed bitmap to that of the returned bitmap from the data changer
            ///*
            dataObj.setYCrCbtoRGB(
                dataChanger.YCbCrtoRGB(
                    dataObj.getRGBtoYCrCb()
                    ));
            //*/
            ShowYButton.Enabled = true;
            showCbButton.Enabled = true;
            ShowCrButton.Enabled = true;
            showYCbCrButton.Enabled = true;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = dataObj.getYCrCbtoRGB();
        }

        private void ShowYButton_Click(object sender, EventArgs e)
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.Image = dataChanger.getYBitmap(pictureBox1.Image);
        }

        private void ShowCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.Image = dataChanger.getCrBitmap(pictureBox1.Image);
        }

        private void showCbButton_Click(object sender, EventArgs e)
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.Image = dataChanger.getCbBitmap(pictureBox1.Image);
        }

        private void showYCbCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            //pictureBox3.Image = dataChanger.getYCbCrBitmap(pictureBox1.Image);
            pictureBox3.Image = dataObj.getRGBtoYCrCb();
        }
    }
}
