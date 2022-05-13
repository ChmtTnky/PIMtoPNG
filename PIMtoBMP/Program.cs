using System.Drawing;

// get file to convert
Console.Write("Input the name of the PIM file (including extension): ");
string pimfile = Console.ReadLine();

// check if file exists
if (!File.Exists(pimfile))
{
    Console.WriteLine("Invalid Filename\nExiting Execution");
    return;
}

// read file data
byte[] data = File.ReadAllBytes(pimfile);

// get important values from header
int width = data[0] + (data[1] * 256);
int height = data[2] + (data[3] * 256);
int bit_depth = data[4] + (data[5] * 256);
int color_count = data[6] + (data[7] * 256);
int palette_start = data[8] + (data[9] * 256);
int pixel_start = data[12] + (data[13] * 256);

// get color palette data
Color[] palette = new Color[color_count];
for (int i = 0; i < color_count; i++)
{
    // alpha values only range from 0x00-0x80, so it gets normalized to the proper 0x00-0xFF range by multiplying by 2
    // 0x80 corresponds to a solid color, and it has to be reduced to 255 after the multiplication
    int alpha = ((data[palette_start + (4 * i) + 3]) * 2);
    if (alpha > 255)
        alpha = 255;
    int red = data[palette_start + (4 * i)];
    int green = data[palette_start + (4 * i) + 1];
    int blue = data[palette_start + (4 * i) + 2];

    palette[i] = Color.FromArgb(alpha, red, green, blue);
}

Bitmap pimbmp = new Bitmap(width, height);
switch (bit_depth)
{
    case 4:
        {
            // make bit array of all pixel data
            bool[] bitdata = new bool[(data.Length - pixel_start) * 8];
            for (int i = pixel_start; i < data.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // gets each bit of the byte in order from highest bit to lowest bit
                    bitdata[((i - pixel_start) * 8) + j] = ((data[i] >> (7 - j)) % 2) == 1;
                }
            }

            // iterates through based on the expected number of pixels in the image.
            for (int p = 0; p < width * height + Convert.ToInt32(width * height % 2); p++)
            {
                int color_index = 0;
                // read 4 bits in order from highest to lowest and get the sum to find color index
                color_index += Convert.ToInt32(bitdata[(p * 4)]) * 8;
                color_index += Convert.ToInt32(bitdata[(p * 4) + 1]) * 4;
                color_index += Convert.ToInt32(bitdata[(p * 4) + 2]) * 2;
                color_index += Convert.ToInt32(bitdata[(p * 4) + 3]);

                // the pixel order is bit wonky and works like this: 1,0,3,2,5,4 \ 7,6,9,8,11,10 \ ...
                // so, the pixel index is adjusted to account for that
                int adjusted_pixel_index = p + (2 * ((p + 1) % 2) - 1);
                // the second to last pixel in an image with an odd number of pixels corresponds
                // to a pixel outside the bounds of the image, but the next one doesn't
                // to account for this, pixels beyond the limits of the image are manually ignored
                if (adjusted_pixel_index >= width * height)
                    continue;
                // convert pixel index to the appropriate width and height within the image
                int pixel_width = adjusted_pixel_index % width;
                int pixel_height = (adjusted_pixel_index - pixel_width) / width;

                // set color of the specified pixel
                pimbmp.SetPixel(pixel_width, pixel_height, palette[color_index]);
            }
            break;
        }
    case 8:
        {
            // this one is much simpler
            // for every pixel, read a byte of data
            // get the color of the index specified by the byte and write it to the image
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int color_index = data[pixel_start + (h * width) + w];
                    pimbmp.SetPixel(w, h, palette[color_index]);
                }
            }
            break;
        }
    case 32:
        {
            // also simple
            // for each pixel read 4 bytes, each of which correspond to one aspect of the color
            // red, green, blue, then alpha
            // for each pixel, make the color and write it to the image
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int red = data[pixel_start + (4 * ((h * width) + w))];
                    int green = data[pixel_start + (4 * ((h * width) + w)) + 1];
                    int blue = data[pixel_start + (4 * ((h * width) + w)) + 2];
                    // normalize the alpha
                    int alpha = data[pixel_start + (4 * ((h * width) + w)) + 3] * 2;
                    if (alpha > 255)
                        alpha = 255;
                    pimbmp.SetPixel(w, h, Color.FromArgb(alpha, red, green, blue));
                }
            }
            break;
        }
    default:
        {
            // if the bit depth is unknown, exit execution
            Console.WriteLine("Unknown Bit-Depth/Incorrect Data Type\nExiting Execution");
            return;
        }
}

// get output file name
string newfilename = pimfile.Remove(pimfile.Length - 4) + ".bmp";

// create new bmp file
pimbmp.Save(newfilename);
if (File.Exists(newfilename))
    Console.WriteLine("New Bitmap Saved as " + newfilename);
else
    Console.WriteLine("Could not create file\nExiting Execution");