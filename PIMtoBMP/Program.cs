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
            bool[] bitdata = new bool[(data.Length - pixel_start) * 8];
            for (int i = pixel_start; i < data.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bitdata[((i - pixel_start) * 8) + j] = ((data[i] >> (7 - j)) % 2) == 1;
                }
            }

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int bit_index = ((h * width) + w) * bit_depth;
                    int color_index = 0;

                    for (int i = 0; i < bit_depth; i++)
                        color_index += (int)(Math.Pow(2, bit_depth - i - 1) * Convert.ToInt32(bitdata[bit_index + i]));

                    pimbmp.SetPixel(w + (2 * ((w + 1) % 2) - 1), h, palette[color_index]);
                }
            }
            break;
        }
    case 8:
        {
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
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int red = data[pixel_start + (4 * ((h * width) + w))];
                    int green = data[pixel_start + (4 * ((h * width) + w)) + 1];
                    int blue = data[pixel_start + (4 * ((h * width) + w)) + 2];
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