using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WMF2WPFCommandLine
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            var recursePath = false;
            var sourcePath = "";
            var destPath = "";
            var format = FormatType.Xaml;

            if (args.Length > 1)
            {
                sourcePath = args[0].Trim(new char[1] { '\"' });
                destPath = args[1].Trim(new char[1] { '\"' });
            }

            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "-r":
                    case "/r":
                        recursePath = true;
                        break;
                    case "-format:png":
                    case "/format:png":
                        format = FormatType.Png;
                        break;
                    case "-format:pngnative":
                    case "/format:pngnative":
                        format = FormatType.PngNative;
                        break;
                }
            }

            if (String.IsNullOrEmpty(sourcePath) | String.IsNullOrEmpty(destPath))
            {
                Console.WriteLine("Invalid source or destination path");
            }

            ConvertFiles(sourcePath, destPath, recursePath, format);
        }

        private static void ConvertFiles(string sourcePath, string destPath, bool recursive, FormatType format)
        {
            var options = SearchOption.TopDirectoryOnly;
            var timeStarted = DateTime.Now;

            if (recursive)
            {
                options = SearchOption.AllDirectories;
            }

            var files = Directory.GetFiles(sourcePath, "*.wmf", options);

            //foreach (string filePath in files)
            //{
            //    ConvertWmfFile(filePath, destPath, format);
            //}
            ParallelForSTA(0, files.Length, i => ConvertWmfFile(sourcePath, files[i], destPath, format));

            Console.WriteLine(DateTime.Now - timeStarted);
        }

        private static void ConvertWmfFile(string sourcePath, string filePath, string destPath, FormatType format)
        {
            System.Windows.Controls.Canvas WPFCanvas = null;
            var convert = new WMFConversion.WMF2WPF();
            var dpiX = 96;
            var dpiY = 96;
            string imageSavePath = null;

            try
            {
                Console.WriteLine("Converting: " + filePath);

                if (format == FormatType.Png)
                {
                    dpiX = 96;
                    dpiY = 96;
                }
                else if (format == FormatType.Xaml)
                {
                    // For xaml we need to save bitmaps to file
                    imageSavePath = Path.Combine(destPath, Path.GetFileNameWithoutExtension(filePath) + "_");
                }

                if (format != FormatType.PngNative)
                {
                    using (var loadStream = new FileStream(filePath, FileMode.Open))
                    {
                        WPFCanvas = convert.Convert(loadStream, dpiX, dpiY, imageSavePath);
                    }
                }


                if (format == FormatType.Png)
                {
                    Directory.CreateDirectory(Path.Combine(destPath,
                                                           Path.GetDirectoryName(
                                                                   filePath.Substring(sourcePath.Length + 1)))
                                                                   );
                    var outStream = new FileStream(Path.Combine(destPath, Path.Combine(Path.GetDirectoryName(filePath.Substring(sourcePath.Length + 1)), Path.GetFileNameWithoutExtension(filePath) + ".png")), FileMode.Create);

                    var renderBitmap =
                      new RenderTargetBitmap(
                        (int)WPFCanvas.Width,
                        (int)WPFCanvas.Height,
                        96d,
                        96d,
                        PixelFormats.Pbgra32);
                    renderBitmap.Render(WPFCanvas);

                    // Use png encoder for our data
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(outStream);
                    outStream.Close();
                    outStream.Dispose();
                }
                else if (format == FormatType.PngNative)
                {
                    Directory.CreateDirectory(Path.Combine(destPath,
                                                              Path.GetDirectoryName(
                                                                      filePath.Substring(sourcePath.Length + 1)))
                                                                      );
                    var outStream = new FileStream(Path.Combine(destPath, Path.Combine(Path.GetDirectoryName(filePath.Substring(sourcePath.Length + 1)), Path.GetFileNameWithoutExtension(filePath) + "_native.png")), FileMode.Create);

                    var metafile1 = new System.Drawing.Imaging.Metafile(filePath);
                    metafile1.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                    outStream.Close();
                    outStream.Dispose();
                }
                else
                {
                    File.WriteAllText(Path.Combine(destPath, Path.GetFileNameWithoutExtension(filePath) + ".xaml"), System.Windows.Markup.XamlWriter.Save(WPFCanvas));
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        public static void ParallelForSTA(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Determine the number of iterations to be processed, the number of 
            // cores to use, and the approximate number of iterations to process 
            // in each thread. 
            int size = exclusiveUpperBound - inclusiveLowerBound;
            int numProcs = Environment.ProcessorCount > 4 ? 4 : Environment.ProcessorCount; // Cap threads at 4
            int range = size / numProcs;

            // Use a thread for each partition. Create them all, 
            // start them all, wait on them all. 
            var threads = new List<Thread>(numProcs);
            for (int p = 0; p < numProcs; p++)
            {
                int start = p * range + inclusiveLowerBound;
                int end = (p == numProcs - 1) ? exclusiveUpperBound : start + range;
                threads.Add(new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                        body(i);
                }));
            }
            foreach (var thread in threads)
            {
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            foreach (var thread in threads) thread.Join();
        }


        private enum FormatType
        {
            Xaml,
            Png,
            PngNative
        }
    }
}
