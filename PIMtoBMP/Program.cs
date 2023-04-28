using System.Drawing;

namespace PIMtoPNG
{
    public class Program
    {
        static void Main(params string[] args)
        {
            Converter con = new Converter();

            if (args.Length == 0)
                con.ConvertFormat(string.Empty);
            else
                con.ConvertFormat(args[0]);
        }
    }
}