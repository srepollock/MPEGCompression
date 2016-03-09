# 4932-Compression
COMP4995 JPG Compression

## Usage
---
### JPEG Loading/Saving
- Load '.bmp', '.jpg', '.png'  
- Save images as '.rippeg' file
- Load '.rippeg' file to check loading
### H.261/MPEG Loading/Saving
- Load picture boxes 1 & 2 under 'Motion Vectors'
    - Load image 1 as '.bmp', '.jpg', '.png' 
    - Load image 2 as '.bmp', '.jpg', '.png'
- Calculate motion vectors under 'Motion Vectors'
- Save motion vectors as a '.mrippeg' file
- Load motion vector files with the file ending '.mrippeg'
    - Will load the I-Frame in picture box 2
    - Will load the calculated P-Frame in picture box 3

## To Do
---
- For motion vectors
	- working
	- need to generate the Cr & Cb motion vectors (just look at how I did luma)
	- need to save and write the motion vectors
	- maybe fix the origin of where the pictures are drawing
- MPEG compression
	- save
		- I frame
			- basically saving the files
		- P frame
		- motion vectors
