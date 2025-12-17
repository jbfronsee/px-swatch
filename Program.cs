using ImageMagick;

internal class Program
{
    public static void GeneratePalette(Options opts, MagickImage inputImage)
    {
        if(opts.ResizePercentage < 100 && opts.ResizePercentage > 0)
        {
            inputImage.Resize(new Percentage(opts.ResizePercentage));
        }

        List<IMagickColor<byte>> palette = Palette.PaletteFromImage(inputImage);

        if (!opts.HistogramOnly)
        {
            palette = Palette.PaletteFromImageKmeans(inputImage, palette);
        }

        if (opts.Print)
        {
            Console.WriteLine("Palette: ");
            foreach(IMagickColor<byte> color in palette)
            {
                Console.WriteLine($"Color: {color.ToHexString()}");
            }
        }
        else if (opts.AsGPL)
        {
            List<string> file = Format.AsGPL(palette, Path.GetFileNameWithoutExtension(opts.OutputFile));
            File.WriteAllLines(opts.OutputFile, file);
        }
        else
        {
            MagickImage paletteImage = Format.AsPNG(palette);

            if (opts.PrintImage)
            {
                paletteImage.Write(Console.OpenStandardOutput());
            }
            else
            {
                paletteImage.Write(opts.OutputFile);
            }
        }
    }

    public static bool HasErrors(Options opts)
    {
        bool hasErrors = false;
        if (string.IsNullOrEmpty(opts.InputFile))
        {
            Console.WriteLine("Usage: px-swatch [InputFile] [flags]");
            hasErrors = true;
        }
        else if (opts.Print == false && opts.PrintImage == false && string.IsNullOrEmpty(opts.OutputFile))
        {
            Console.WriteLine("Missing output file specified with -o [filepath]");
            hasErrors = true;
        }
        else if (!string.IsNullOrEmpty(opts.InvalidArg))
        {
            Console.WriteLine($"Invalid Argument: {opts.InvalidArg}");
            hasErrors = true;
        }

        return hasErrors;
    }

    private static void Main(string[] args)
    {
        Options opts = Options.GetOptions(args);

        if (HasErrors(opts))
        {
            return;
        }

        var inputImage = new MagickImage(opts.InputFile);

        if (opts.Print || opts.Verbose)
        {
            Console.WriteLine("Processing Image...");
        }

        if (opts.Verbose)
        {
            var histogram = inputImage.Histogram();
            foreach (var color in histogram)
            {
                Console.WriteLine($"Color: {color.Key} Occurences: {color.Value}");
            }
        }

        GeneratePalette(opts, inputImage);
    }
}