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
    /// <summary>
    /// File Loader
    /// This is the main program class
    /// </summary>
    /// <remarks>
    /// Defining all the basic functions to run the program, and button calls.
    /// </remarks>
    public partial class FileLoader : Form
    {
        Data dataObj;
        RGBChanger dataChanger;
        DCT dctObj;
        ZigZag zz;
        Blocks block;
        Quantize q;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// This function will be called on the startup of the window. It 
        /// initializes the objects for the data, dct, zigzag, block, and 
        /// quantize class. It also turns off all the buttons that should 
        /// not be pressed right away.
        /// </remarks>
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
            calculateMotionVectorsToolStripMenuItem.Enabled = false;
        }
        /// <summary>
        /// Load in the files specified by the user. 
        /// </summary>
        /// <remarks>
        /// It will then turn on all buttons allowed when opening that
        /// type of file.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    rgbChangeButton.Enabled = true;
                }
                ShowYButton.Enabled = false;
                showCbButton.Enabled = false;
                ShowCrButton.Enabled = false;
                showYCbCrButton.Enabled = false;
            }
        }
        /// <summary>
        /// This function opens a file save dialog.
        /// </summary>
        /// <remarks>
        /// Calls the saveFile method.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Removes the pictures from all the picture boxes
        /// </summary>
        /// <remarks>
        /// Turns off all buttons that should not be allowed.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearPicturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            rgbChangeButton.Enabled = false;
            ShowYButton.Enabled = false;
            showCbButton.Enabled = false;
            ShowCrButton.Enabled = false;
            showYCbCrButton.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            clearPicturesToolStripMenuItem.Enabled = false;
            fileNameBox.Text = null;
            dataObj = new Data();
        }
        /// <summary>
        /// Changes data from RGB to YCbCr and back, displays in picturebox2
        /// </summary>
        /// <remarks>
        /// This function changes the image on the left (loaded in by the user)
        /// to a YCbCr format, and back. This will just display the image
        /// in RGB format, but behind the scenes, sets up the data for
        /// YCbCr display, subsamples, and sets up data to save the image to
        /// file format with a header after being run through Run Length
        /// Encoding.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rgbChangeButton_Click(object sender, EventArgs e)
        {
            int sz = 0;
            byte[,] tempY, tempCb, tempCr;
            sbyte[,] stempY, stempCb, stempCr;
            double[,] tempDY, tempDCb, tempDCr;
            sbyte[] szztempY, szztempB, szztempR;
            /* This data needs to be saved for the header information */
            Pad padding = new Pad(ref dataObj);
            dataChanger.RGBtoYCbCr(dataObj.getOriginal(), ref dataObj); // This will set the data changed bitmap to that of the returned bitmap from the data changer
            // pad data
            dataObj.setyData(padding.padData(dataObj.getyData(), padding.padW, padding.padH, dataObj));
            dataObj.setCbData(padding.padData(dataObj.getCbData(), padding.padW, padding.padH, dataObj));
            dataObj.setCrData(padding.padData(dataObj.getCrData(), padding.padW, padding.padH, dataObj));
            // setup arrays
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
                    stempY = q.quantizeLuma(tempDY);
                    // zigzag
                    szztempY = zz.zigzag(stempY);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.yEncoded, sz);
                    Buffer.BlockCopy(szztempY, 0, dataObj.yEncoded, pos, 64);
                    // unzigzag
                    stempY = zz.unzigzag(szztempY);
                    // inverse quantize
                    tempDY = q.inverseQuantizeLuma(stempY);
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
                    stempCb = q.quantizeData(tempDCb);
                    // zigzag
                    szztempB = zz.zigzag(stempCb);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.cbEncoded, sz);
                    Buffer.BlockCopy(szztempB, 0, dataObj.cbEncoded, pos, 64);
                    // unzigzag
                    stempCb = zz.unzigzag(szztempB);
                    // inverse quantize
                    tempDCb = q.inverseQuantizeData(stempCb);
                    tempCb = dctObj.inverseDCTByte(tempDCb);
                    block.putback(dataObj.getCbData(), tempCb, i, j);

                    // Cr (data is subsampled)
                    tempCr = block.generate2DBlocks(dataObj.getCrData(), i, j);
                    tempDCr = dctObj.forwardDCT(tempCr);
                    // quantize
                    stempCr = q.quantizeData(tempDCr);
                    // zigzag
                    szztempR = zz.zigzag(stempCr);

                    // put the data into the final array here with an offset of i+=64 for each array
                    Array.Resize<sbyte>(ref dataObj.crEncoded, sz);
                    Buffer.BlockCopy(szztempR, 0, dataObj.crEncoded, pos, 64);
                    // unzigzag
                    stempCr = zz.unzigzag(szztempR);
                    // inverse quantize
                    tempDCr = q.inverseQuantizeData(stempCr);
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
        /// <summary>
        /// Loads image into picturebox1 for MotionVectors only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadImage1ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP Files|*.bmp|JPG Files|*.jpg|PNG Files|*.png|RIPPEG Files|*.rippeg|All Files|*.*";
            DialogResult result = openFileDialog.ShowDialog(); // I want to open this to the child window in the file
            if (result == DialogResult.OK) // checks if the result returned true
            {
                string ext = Path.GetExtension(openFileDialog.FileName); // includes the period
                if (ext == ".rippeg")
                {
                    openFile(openFileDialog.FileName, pictureBox1);
                    dataObj.mv1Head.setHeight((short)pictureBox1.Image.Height);
                    dataObj.mv1Head.setWidth((short)pictureBox1.Image.Width);
                }
                else
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                    dataObj.mv1Head.setHeight((short)pictureBox1.Image.Height);
                    dataObj.mv1Head.setWidth((short)pictureBox1.Image.Width);
                }
                if (pictureBox1.Image != null && pictureBox2.Image != null)
                    calculateMotionVectorsToolStripMenuItem.Enabled = true;
            }
        }
        /// <summary>
        /// Loads image into picturebox2 for MotionVectors only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadImage2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMP Files|*.bmp|JPG Files|*.jpg|PNG Files|*.png|RIPPEG Files|*.rippeg|All Files|*.*";
            DialogResult result = openFileDialog.ShowDialog(); // I want to open this to the child window in the file
            if (result == DialogResult.OK) // checks if the result returned true
            {
                string ext = Path.GetExtension(openFileDialog.FileName); // includes the period
                if (ext == ".rippeg")
                {
                    openFile(openFileDialog.FileName, pictureBox2);
                    dataObj.mv2Head.setHeight((short)pictureBox2.Image.Height);
                    dataObj.mv2Head.setWidth((short)pictureBox2.Image.Width);
                }
                else
                {
                    pictureBox2.Image = Image.FromFile(openFileDialog.FileName);
                    dataObj.mv2Head.setHeight((short)pictureBox2.Image.Height);
                    dataObj.mv2Head.setWidth((short)pictureBox2.Image.Width);
                }
                if (pictureBox1.Image != null && pictureBox2.Image != null)
                    calculateMotionVectorsToolStripMenuItem.Enabled = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calculateMotionVectorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // need to get YCbCr data of img 1 and 2
            // Image 1
                // data saved into the Data.yData, cbData, crData
            // Image 2
                // data saved into the Data.yData2, cbData2, crData2
            dataChanger.RGBtoYCbCr((Bitmap)pictureBox1.Image, ref dataObj, 1);
            dataChanger.RGBtoYCbCr((Bitmap)pictureBox2.Image, ref dataObj, 2);
            // Create bitmap for picturebox 3
            Bitmap bmp = new Bitmap(dataObj.mv1Head.getWidth(), dataObj.mv1Head.getHeight());

            // Current is picturebox2 (C Frame) (right image)
            // Reference is picturebox1 (R Frame) (left image)
                // data is YCbCr data
                // how big are the macroblocks? Are these my 8x8 blocks? Ask Austin
                    // Where does the MAD data go?
                    // How do I decide on N, or p?
            

            MotionVector mv = new MotionVector();
            
            // draw lines where the changes are
            using (var graphics = Graphics.FromImage(bmp))
            {
                Pen blackPen = new Pen(Color.Black, 3);
                // just draw the motion vector(x,y)(x1,y1) coords
                graphics.DrawLine(blackPen, mv.x1, mv.y1, mv.x2, mv.y2);
            }

            // Set bitmap for picturebox3
            pictureBox3.Image = bmp;
        }
        /// <summary>
        /// Sets final data array with Y + Cb + Cr data (in order) after being RLE'ed
        /// </summary>
        /// <remarks>
        /// This function sets the final data to be outputted to a file into
        /// a single array. This will make it easier to save the data by
        /// only calling on one array, instead of 3 each of different
        /// sizes.
        /// </remarks>
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
        /// <summary>
        /// Splits data from a single array to Y, Cb & Cr depending on the size
        /// specified in header.
        /// </summary>
        /// <remarks>
        /// This splits the data up that has been read into the program
        /// from a file. It splits the data up based on the size of the
        /// arrays read in from the header.
        /// </remarks>
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
        /// <summary>
        /// Shows the Y data in picturebox3
        /// </summary>
        /// <remarks>
        /// Shows the Luminance of the image.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getYBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Shows the Cr data in picturebox3
        /// </summary>
        /// <remarks>
        /// Shows the Chroma Red of the image.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCrBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Shows the Cb data in picturebox3
        /// </summary>
        /// <remarks>
        /// Shows the Chroma Blue of the image.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showCbButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCbBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Shows the YCbCr image in picturebox3
        /// </summary>
        /// <remarks>
        /// Shows the YCbCr of the image.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showYCbCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getYCbCrBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Calculates motion vectors based on display 1 and 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void motionVectorButton_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Saves the YCbCr RLE'ed data to the file name with the gHead in the
        /// data object.
        /// </summary>
        /// <remarks>
        /// Called when we want to save the file, to the specified file name.
        /// This is used after the image has been changed to YCbCr and back,
        /// so that we can have a better compression when saving the data and
        /// using RLE.
        /// </remarks>
        /// <param name="fileName">File name to the data to</param>
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
        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <remarks>
        /// This opens the file of the specified name. This will only be
        /// called on files that end with *.rippeg. (Why? Because it's funny)
        /// Defaults to pictureBox2
        /// </remarks>
        /// <param name="fileName">File name to load data from</param>
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
                    tempDY = q.inverseQuantizeLuma(stempY);
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
                    tempDCb = q.inverseQuantizeData(stempCb);
                    tempDCb = dctObj.dinverseDCT(tempDCb);
                    block.putbackd(dataObj.getdCbData(), tempDCb, x, y);

                    // Cr
                    // block
                    tempCr = block.generateBlocks(dataObj.crEncoded, pos);
                    // unzigzag
                    stempCr = zz.unzigzag(tempCr);
                    // inverse quantize
                    tempDCr = q.inverseQuantizeData(stempCr);
                    tempDCr = dctObj.dinverseDCT(tempDCr);
                    block.putbackd(dataObj.getdCrData(), tempDCr, x, y);
                    pos += 64;
                }
            }
            re.Close();
            // set pixels
            dataObj.setdCbData(Sampler.upsample(dataObj.dCbData, ref dataObj));
            dataObj.setdCrData(Sampler.upsample(dataObj.dCrData, ref dataObj));
            dataChanger.sYCbCrtoRGB(ref dataObj);
            dataChanger = new RGBChanger();
            pictureBox2.Image = dataObj.generateBitmap();
        }
        /// <summary>
        /// Opens the file into the picture box.
        /// </summary>
        /// <remarks>
        /// This opens the file of the specified name. This will only be
        /// called on files that end with *.rippeg. (Why? Because it's funny)
        /// Saves it to the specified picture box.
        /// </remarks>
        /// <param name="fileName">File name to load data from</param>
        public void openFile(string fileName, PictureBox pb)
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
                    tempDY = q.inverseQuantizeLuma(stempY);
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
                    tempDCb = q.inverseQuantizeData(stempCb);
                    tempDCb = dctObj.dinverseDCT(tempDCb);
                    block.putbackd(dataObj.getdCbData(), tempDCb, x, y);

                    // Cr
                    // block
                    tempCr = block.generateBlocks(dataObj.crEncoded, pos);
                    // unzigzag
                    stempCr = zz.unzigzag(tempCr);
                    // inverse quantize
                    tempDCr = q.inverseQuantizeData(stempCr);
                    tempDCr = dctObj.dinverseDCT(tempDCr);
                    block.putbackd(dataObj.getdCrData(), tempDCr, x, y);
                    pos += 64;
                }
            }
            re.Close();
            // set pixels
            dataObj.setdCbData(Sampler.upsample(dataObj.dCbData, ref dataObj));
            dataObj.setdCrData(Sampler.upsample(dataObj.dCrData, ref dataObj));
            dataChanger.sYCbCrtoRGB(ref dataObj);
            dataChanger = new RGBChanger();
            pb.Image = dataObj.generateBitmap();
        }
        /// <summary>
        /// Writes the header data to the file
        /// </summary>
        /// <remarks>
        /// Writes the header data. This data comes from after we have changed
        /// the data from RGB to YCrCb.
        /// </remarks>
        /// <param name="file">File to write to</param>
        /// <param name="header">Header information</param>
        private void writeData(BinaryWriter file, Header header)
        {
            file.Write(header.getHeight());
            file.Write(header.getWidth());
            file.Write(header.getQuality());
            file.Write(header.getYlen());
            file.Write(header.getCblen());
            file.Write(header.getCrlen());
        }
        /// <summary>
        /// Writes the final data to the file
        /// </summary>
        /// <remarks>
        /// Writes the final data, loaded earlier into the Data object, and
        /// writes the data as sbytes to the file after being RLE'ed.
        /// </remarks>
        /// <param name="file">File to write to</param>
        /// <param name="header">Header information for data size</param>
        /// <param name="data">Data to write</param>
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
        /// <summary>
        /// Reads the header data into the Data object.
        /// </summary>
        /// <remarks>
        /// Reads data into the header object frome the specifed file.
        /// Necessary to read the data in properly.
        /// </remarks>
        /// <param name="file">File to read data in from</param>
        /// <param name="header">Header to read the data into</param>
        private void readData(BinaryReader file, Header header)
        {
            header.setHeight(file.ReadInt16());
            header.setWidth(file.ReadInt16());
            header.setQuality(file.ReadByte());
            header.setYlen(file.ReadInt32());
            header.setCblen(file.ReadInt32());
            header.setCrlen(file.ReadInt32());
        }
        /// <summary>
        /// Reads the rest of the data into the final data array to be split.
        /// </summary>
        /// <remarks>
        /// Reads data from the file into the data array. This will read in
        /// the data based on the 3 array sizes we have saved specified in
        /// the header which we also pass in.
        /// </remarks>
        /// <param name="file">File to read data in from</param>
        /// <param name="header">Header for the array sizes</param>
        /// <param name="data">Data array to read into</param>
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
