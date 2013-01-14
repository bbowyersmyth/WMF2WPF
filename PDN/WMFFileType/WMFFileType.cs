using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PaintDotNet;

namespace WMFFileType
{
    public class WMFFileType : FileType
    {
        private Document _document;

        public WMFFileType()
            : base("Windows Metafile",
                FileTypeFlags.SupportsLoading,
                new String[] { ".wmf", ".wmz" })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            return ConvertFile(input);
        }

        public Document ConvertFile(Stream input)
        {
            try
            {
                // WPF controls require an STA thread
                var worker = new Thread(new ParameterizedThreadStart(ConvertStream));
                worker.SetApartmentState(ApartmentState.STA);
                worker.Name = "WMFConvert";
                worker.Start(input);
                worker.Join();

                return this._document;
            }
            catch
            {
                MessageBox.Show("Problem opening File");

                var b = new Bitmap(500, 500);
                return Document.FromImage(b);
            }
        }

        private void ConvertStream(object input)
        {
            var convert = new WMFConversion.WMF2WPF();
            var outStream = new MemoryStream();
            var dpiX = 96;
            var dpiY = 96;

            try
            {
                var WPFCanvas = convert.Convert((Stream)input, dpiX, dpiY);

                if (WPFCanvas.Width == 0.0)
                    WPFCanvas.Width = 1000;

                if (WPFCanvas.Height == 0.0)
                    WPFCanvas.Height = 1000;

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
                outStream.Position = 0;

                Image img = new Bitmap(outStream);

                this._document = Document.FromImage(img);
                ((Layer)this._document.Layers[0]).IsBackground = false;
                ((Layer)this._document.Layers[0]).Name = "Metafile";
                this._document.Layers.Insert(0, Layer.CreateBackgroundLayer(img.Width, img.Height));
            }
            catch
            {
            }
        }
    }
}
