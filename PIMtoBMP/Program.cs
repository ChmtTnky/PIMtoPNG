using System.Drawing;

const int PALETTE_START = 0x10;
const int PIXEL_START = 0x410;

// get file to convert
Console.Write("Input the name of the PIM file (including extension): ");
string pimfile = Console.ReadLine();

// check if file exists
if (!File.Exists(pimfile))
{
    Console.WriteLine("Invalid Filename");
    return;
}

// read file data
byte[] data = File.ReadAllBytes(pimfile);

int width = data[0] + (data[1] * 256);
int height = data[2] + (data[3] * 256);

// get color palette data
bool valid = true;
Color[] palette = Array.Empty<Color>();
for (int i = 0; valid; i++)
{
    int alpha = ((data[PALETTE_START + (4 * i) + 3]) * 2);
    if (alpha > 255)
        alpha = 255;
    int red = data[PALETTE_START + (4 * i)];
    int green = data[PALETTE_START + (4 * i) + 1];
    int blue = data[PALETTE_START + (4 * i) + 2];

    if ((alpha + red + green + blue == 0 && i > 0) || i > 255)
    {
        valid = false;
    }
    else
    {
        Array.Resize(ref palette, palette.Length + 1);
        palette[i] = Color.FromArgb(alpha, red, green, blue);
    }
}

// set pixel data
Bitmap pimbmp = new Bitmap(width, height);
for (int h = 0; h < height; h++)
{
    for (int w = 0; w < width; w++)
    {
        int index = data[PIXEL_START + (h * width) + w];

        pimbmp.SetPixel(w, h, palette[index]);
    }
}

// get output file name
string newfilename = string.Empty;
for (int i = 0; i < pimfile.Length - 4; i++)
    newfilename += pimfile[i];
newfilename += ".bmp";

// create new bmp file
pimbmp.Save(newfilename);
if (File.Exists(newfilename))
    Console.WriteLine("New Bitmap Saved as " + newfilename);