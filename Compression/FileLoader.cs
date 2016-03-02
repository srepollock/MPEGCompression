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
    /// This is the main program class, defining all the basic functions
    /// to run the program, and button calls.
    /// </summary>
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

        /// <summary>
        /// File Loader
        /// This function will be called on the startup of the window. It 
        /// initializes the objects for the data, dct, zigzag, block, and 
        /// quantize class. It also turns off all the buttons that should 
        /// not be pressed right away.
        /// </summary>
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
        /// <summary>
        /// Load Files
        /// This function will load in the files specified by the user. 
        /// It will then turn on all buttons allowed when opening that
        /// type of file.
        /// </summary>
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
        /// Save File
        /// This function opens a file save dialog, (to specify the file name).
        /// Calls the saveFile method.
        /// </summary>
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
        /// Clear Pictures
        /// Removes the pictures from all the picture boxes, and will turn
        /// off all buttons that should not be allowed.
        /// </summary>
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
            fileNameBox.Text = null;
            dataObj = new Data();
        }
        /// <summary>
        /// RGBChangeButton
        /// This function changes the image on the left (loaded in by the user)
        /// to a YCbCr format, and back. This will just display the image
        /// in RGB format, but behind the scenes, sets up the data for
        /// YCbCr display, subsamples, and sets up data to save the image to
        /// file format with a header after being run through Run Length
        /// Encoding.
        /// </summary>
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
        /// Set Final Data
        /// This function sets the final data to be outputted to a file into
        /// a single array. This will make it easier to save the data by
        /// only calling on one array, instead of 3 each of different
        /// sizes.
        /// </summary>
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
        /// Split Final Data
        /// This splits the data up that has been read into the program
        /// from a file. It splits the data up based on the size of the
        /// arrays read in from the header.
        /// </summary>
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
        /// Padd data by a certain width and height. This will ensure
        /// that the data is divisable by 8 and 2. Because the data is cut
        /// in half, and then needs to be cut up into 8x8 blocks, it needs 
        /// to be a mod of 16 (explained in the Pad class).
        /// </summary>
        /// <param name="data">Original data</param>
        /// <param name="padxby">Pad the width by x amount</param>
        /// <param name="padyby">Pad the height by y amount</param>
        /// <returns></returns>
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
        /// <summary>
        /// Padd data by a certain width and height. This will ensure
        /// that the data is divisable by 8 and 2. Because the data is cut
        /// in half, and then needs to be cut up into 8x8 blocks, it needs 
        /// to be a mod of 16 (explained in the Pad class). This is used on
        /// data that is already halved.
        /// 
        /// * I don't belive I use this class any more *
        /// </summary>
        /// <param name="data">Original data</param>
        /// <param name="padxby">Pad the width by x amount</param>
        /// <param name="padyby">Pad the height by y amount</param>
        /// <returns></returns>
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
        /// <summary>
        /// Show Y button
        /// Shows the Luminance of the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getYBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Show Cr button
        /// Shows the Chroma red of the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCrBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Show Cb button
        /// Shows the Chroma blue of the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showCbButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getCbBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Show YCbCr button
        /// Shows the YCbCr of the image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showYCbCrButton_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = dataObj.getYCbCrBitmap(dataObj.gHead);
        }
        /// <summary>
        /// Save File
        /// Called when we want to save the file, to the specified file name.
        /// This is used after the image has been changed to YCbCr and back,
        /// so that we can have a better compression when saving the data and
        /// using RLE.
        /// </summary>
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
        /// Open File
        /// This opens the file of the specified name. This will only be
        /// called on files that end with *.rippeg. (Why? Because it's funny)
        /// </summary>
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

            dataChanger.sYCbCrtoRGB(
                ref dataObj
                );
            dataChanger = new RGBChanger();
            pictureBox2.Image = dataObj.generateBitmap();
        }
        /// <summary>
        /// Write Data
        /// Writes the header data. This data comes from after we have changed
        /// the data from RGB to YCrCb.
        /// </summary>
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
        /// Write Data
        /// Writes the final data, loaded earlier into the Data object, and
        /// writes the data as sbytes to the file after being RLE'ed.
        /// </summary>
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
        /// Read Data
        /// Reads data into the header object frome the specifed file.
        /// Necessary to read the data in properly.
        /// </summary>
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
        /// Read Data
        /// Reads data from the file into the data array. This will read in
        /// the data based on the 3 array sizes we have saved specified in
        /// the header which we also pass in.
        /// </summary>
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
