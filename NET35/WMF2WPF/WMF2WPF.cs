using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WPFGDI;
#if NETFX_CORE
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace WMFConversion
{
    public class WMF2WPF
    {
        private bool _isLittleEndian;
        private Canvas _rootCanvas;
        private Rect _boundingBox;
        private double _scale;
        private WpfGdi _currentDC;
        private int _dpiX = 96;
        private int _dpiY = 96;
        private string _imageSavePath;

        public Canvas Convert(Stream input, int dpiX, int dpiY, string imageSavePath)
        {
            _dpiX = dpiX;
            _dpiY = dpiY;
            _imageSavePath = imageSavePath;

            return Convert(input);
        }

        public Canvas Convert(Stream input, int dpiX, int dpiY)
        {
            _dpiX = dpiX;
            _dpiY = dpiY;

            return Convert(input);
        }

        public Canvas Convert(Stream input)
        {
            var wmfStream = new MemoryStream();
            BinaryReader wmfReader;
            var eof = false;

            // Copy stream
            const int size = 4096;
            var bytes = new byte[4096];
            int numBytes;

            while ((numBytes = input.Read(bytes, 0, size)) > 0)
                wmfStream.Write(bytes, 0, numBytes);

            //.Net 4.0
            //input.CopyTo(output); 

            _isLittleEndian = BitConverter.IsLittleEndian;
            _rootCanvas = new Canvas
            {
#if NETFX_CORE
                Background = new SolidColorBrush()
#else
                Background = Brushes.Transparent
#endif
            };
            _currentDC = new WpfGdi();

#if !NETFX_CORE
            _currentDC.ImageSavePath = _imageSavePath;
#endif

            wmfStream.Position = 0;

            // Check for wmz file
            wmfStream = UncompressWmz(wmfStream);
            wmfReader = new BinaryReader(wmfStream);

            // Check for placeable header
            uint key = EndianFlip(wmfReader.ReadUInt32());

            if (key == 0x9AC6CDD7)
            {
                // Placeable WMF
                ReadPlaceableWMFHeader(wmfReader);
            }
            else
            {
                // Standard WMF, reset position
                wmfStream.Position = 0;
            }

            ReadWMFHeader(wmfReader);

            while (!eof)
            {
                var record = ReadRecord(wmfReader);

                System.Diagnostics.Debug.WriteLine(record.Function.ToString());

                switch (record.Function)
                {
                    case RecordType.META_EOF:
                        eof = true;
                        break;

                    case RecordType.META_ARC:
                        ReadArc(wmfReader);
                        break;

                    case RecordType.META_CREATEBRUSHINDIRECT:
                        ReadCreateBrushIndirect(wmfReader);
                        break;

                    case RecordType.META_CREATEFONTINDIRECT:
                        ReadCreateFontIndirect(wmfReader, record.Size);
                        break;

                    case RecordType.META_CREATEPALETTE:
                        ReadCreatePalette(wmfReader);
                        break;

                    case RecordType.META_CREATEPATTERNBRUSH:
                        ReadCreatePatternBrush(wmfReader);
                        break;

                    case RecordType.META_CREATEPENINDIRECT:
                        ReadCreatePenIndirect(wmfReader, record.Size);
                        break;

                    case RecordType.META_CREATEREGION:
                        ReadCreateRegion(wmfReader);
                        break;

                    case RecordType.META_DELETEOBJECT:
                        ReadDeleteObject(wmfReader);
                        break;

                    case RecordType.META_DIBBITBLT:
                        ReadDibBitBlt(wmfReader, record.Size);
                        break;

                    case RecordType.META_DIBCREATEPATTERNBRUSH:
                        ReadCreateDibPatternBrush(wmfReader, record.Size);
                        break;

                    case RecordType.META_DIBSTRETCHBLT:
                        ReadDibStretchBlt(wmfReader, record.Size);
                        break;

                    case RecordType.META_ELLIPSE:
                        ReadEllipse(wmfReader);
                        break;

                    case RecordType.META_ESCAPE:
                        ReadEscape(wmfReader, record.Size);
                        break;

                    case RecordType.META_EXCLUDECLIPRECT:
                        ReadExcludeClipRect(wmfReader);
                        break;

                    case RecordType.META_EXTTEXTOUT:
                        ReadExtTextOut(wmfReader, record.Size);
                        break;

                    case RecordType.META_INTERSECTCLIPRECT:
                        ReadIntersectClipRect(wmfReader);
                        break;

                    case RecordType.META_LINETO:
                        ReadLineTo(wmfReader);
                        break;

                    case RecordType.META_MOVETO:
                        ReadMoveTo(wmfReader);
                        break;

                    case RecordType.META_POLYGON:
                        ReadPolygon(wmfReader);
                        break;

                    case RecordType.META_PIE:
                        ReadPie(wmfReader);
                        break;

                    case RecordType.META_POLYLINE:
                        ReadPolyline(wmfReader);
                        break;

                    case RecordType.META_POLYPOLYGON:
                        ReadPolyPolygon(wmfReader);
                        break;

                    case RecordType.META_REALIZEPALETTE:
                        _currentDC.RealizePalette();
                        break;

                    case RecordType.META_RECTANGLE:
                        ReadRectangle(wmfReader);
                        break;

                    case RecordType.META_RESIZEPALETTE:
                        ReadResizePalette(wmfReader);
                        break;

                    case RecordType.META_RESTOREDC:
                        ReadRestoreDC(wmfReader);
                        break;

                    case RecordType.META_ROUNDRECT:
                        ReadRoundRectange(wmfReader);
                        break;

                    case RecordType.META_SAVEDC:
                        ReadSaveDC();
                        break;

                    case RecordType.META_SELECTCLIPREGION:
                        ReadSelectClipRegion(wmfReader);
                        break;

                    case RecordType.META_SELECTOBJECT:
                        ReadSelectObject(wmfReader);
                        break;

                    case RecordType.META_SELECTPALETTE:
                        ReadSelectPalette(wmfReader);
                        break;

                    case RecordType.META_SETBKCOLOR:
                        ReadSetBkColor(wmfReader);
                        break;

                    case RecordType.META_SETBKMODE:
                        _currentDC.SetBkMode((MixMode)EndianFlip(wmfReader.ReadUInt16()));

                        if (record.Size > 4)
                        {
                            wmfReader.ReadBytes(2); // Optional unused field
                        }
                        break;

                    case RecordType.META_SETMAPMODE:
                        _currentDC.SetMapMode((MapMode)EndianFlip(wmfReader.ReadUInt16()));
                        break;

                    case RecordType.META_SETPOLYFILLMODE:
                        _currentDC.SetPolyFillMode((PolyFillMode)EndianFlip(wmfReader.ReadUInt16()));

                        if (record.Size > 4)
                        {
                            wmfReader.ReadBytes(2); // Optional unused field
                        }
                        break;

                    case RecordType.META_SETROP2:
                        _currentDC.SetRop2((BinaryRasterOperation)EndianFlip(wmfReader.ReadUInt16()));

                        if (record.Size > 4)
                        {
                            wmfReader.ReadBytes(2); // Optional unused field
                        }
                        break;


                    case RecordType.META_SETSTRETCHBLTMODE:
                        _currentDC.SetStretchBltMode((StretchMode)EndianFlip(wmfReader.ReadUInt16()));

                        if (record.Size > 4)
                        {
                            wmfReader.ReadBytes(2); // Optional unused field
                        }
                        break;


                    case RecordType.META_SETTEXTALIGN:
                        _currentDC.SetTextAlign(EndianFlip(wmfReader.ReadUInt16()));

                        if (record.Size > 4)
                        {
                            wmfReader.ReadBytes(2); // Optional unused field
                        }
                        break;

                    case RecordType.META_SETTEXTCOLOR:
                        ReadSetTextColor(wmfReader);
                        break;

                    case RecordType.META_SETWINDOWEXT:
                        double extY = EndianFlip(wmfReader.ReadInt16());
                        double extX = EndianFlip(wmfReader.ReadInt16());
                        _currentDC.SetWindowExt(extX, extY);
                        break;

                    case RecordType.META_SETWINDOWORG:
                        double orgY = EndianFlip(wmfReader.ReadInt16());
                        double orgX = EndianFlip(wmfReader.ReadInt16());
                        _currentDC.SetWindowOrg(orgX, orgY);
                        break;

                    case RecordType.META_STRETCHDIB:
                        ReadStretchDib(wmfReader, record.Size);
                        break;

                    case RecordType.META_TEXTOUT:
                        ReadTextOut(wmfReader, record.Size);
                        break;

                    default:
                        // Unsupported record type
                        System.Diagnostics.Debug.WriteLine("^ UNSUPPORTED ^");

                        wmfReader.ReadBytes((int)record.Size * 2 - 6);
                        break;
                }
            }

            // Size canvas
            if (_boundingBox.Width == 0.0 | _boundingBox.Height == 0.0)
            {
                Size windowExt = _currentDC.GetWindowExt();

                if (windowExt.Width == 0.0 | windowExt.Height == 0.0)
                {
                    // TODO
                    _rootCanvas.Width = 800;
                    _rootCanvas.Height = 600;
                }
                else
                {
                    _rootCanvas.Width = windowExt.Width;
                    _rootCanvas.Height = windowExt.Height;
                }
            }
            else
            {
                if (_dpiX == 0)
                {
                    _rootCanvas.Width = _boundingBox.Width * _scale;
                    _rootCanvas.Height = _boundingBox.Height * _scale;
                }
                else
                {
                    _rootCanvas.Width = Math.Round(
                        _boundingBox.Width / 1440 * _dpiX * _scale,
                        0,
                        MidpointRounding.ToEven);
                    _rootCanvas.Height = Math.Round(
                        _boundingBox.Height / 1440 * _dpiY * _scale,
                        0,
                        MidpointRounding.ToEven);
                }
            }

            // Arrange
            var area = new Size(_rootCanvas.Width, _rootCanvas.Height);
            _rootCanvas.Measure(area);
            _rootCanvas.Arrange(new Rect
                    {
                        Height = area.Height,
                        Width = area.Width
                    }
                    );
            _rootCanvas.UpdateLayout();

            return _rootCanvas;
        }

        private static string AsciiBytesToString(byte[] buffer)
        {
            var maxIndex = buffer.Length;

            for (int i = 0; i < maxIndex; i++)
            {
                // Skip non-nulls. 
                if (buffer[i] != 0) continue;
                // First null we find, return the string. 
                maxIndex = i;
                break;
            }

            // Convert the entire section from offset to maxLength. 
            return System.Text.Encoding.GetEncoding("Windows-1252").GetString(buffer, 0, maxIndex);
        }

        private string CharsetBytesToString(byte[] buffer)
        {
            var maxIndex = buffer.Length;
            var encoding = "Windows-1252";

            for (int i = 0; i < maxIndex; i++)
            {
                // Skip non-nulls. 
                if (buffer[i] != 0) continue;
                // First null we find, return the string. 
                maxIndex = i;
                break;
            }

            switch (_currentDC.GetCharSet())
            {
                case 2: // SYMBOL_CHARSET 
                    encoding = "CP_SYMBOL";
                    break;

                case 128: // SHIFTJI_CHARSET 
                    encoding = "shift_jis";
                    break;

                case 129: // HANGEUL_CHARSET or HANGUL_CHARSET  
                    encoding = "ks_c_5601-1987";
                    break;

                case 130: // JOHAB_CHARSET  
                    encoding = "Johab";
                    break;

                case 134: // GB2312_CHARSET  
                    encoding = "gb2312";
                    break;

                case 136: // CHINESEBIG5_CHARSET 
                    encoding = "big5";
                    break;

                case 161: // GREEK_CHARSET
                    encoding = "windows-1253";
                    break;

                case 162: // TURKISH_CHARSET
                    encoding = "windows-1254";
                    break;

                case 163: // VIETNAMESE_CHARSET
                    encoding = "windows-1258";
                    break;

                case 177: // HEBREW_CHARSET
                    encoding = "windows-1255";
                    break;

                case 178: // ARABIC_CHARSET
                    encoding = "windows-1256";
                    break;

                case 186: // BALTIC_CHARSET
                    encoding = "windows-1257";
                    break;

                case 204: // RUSSIAN_CHARSET
                    encoding = "windows-1251";
                    break;

                case 222: // THAI_CHARSET
                    encoding = "windows-874";
                    break;

                case 255: // OEM_CHARSET
                    encoding = "IBM437";
                    break;

                case 268: // EASTEUROPE_CHARSET
                    encoding = "windows-1250";
                    break;
            }

            // Convert the entire section from offset to maxLength. 
            return System.Text.Encoding.GetEncoding(encoding).GetString(buffer, 0, maxIndex);
        }

        private ushort EndianFlip(ushort value)
        {
            if (_isLittleEndian)
            {
                return value;
            }
            else
            {
                return (ushort)(((value & 0x00FF) << 8)
                    + ((value & 0xFF00) >> 8));
            }
        }

        private short EndianFlip(short value)
        {
            if (_isLittleEndian)
            {
                return value;
            }
            else
            {
                return (short)(((value & 0x00FF) << 8)
                    + ((value & 0xFF00) >> 8));
            }
        }

        private uint EndianFlip(uint value)
        {
            if (_isLittleEndian)
            {
                return value;
            }
            else
            {
                return (uint)(((value & 0x000000FF) << 24)
                    + ((value & 0x0000FF00) << 8)
                    + ((value & 0x00FF0000) >> 8)
                    + ((value & 0xFF000000) >> 24));
            }
        }

        private UInt64 EndianFlip(UInt64 value)
        {
            if (_isLittleEndian)
            {
                return value;
            }
            else
            {
                return (UInt64)(((0x00000000000000FF) & (value << 56)
                    + (0x000000000000FF00) & (value << 40)
                    + (0x0000000000FF0000) & (value << 24)
                    + (0x00000000FF000000) & (value << 8)
                    + (0x000000FF00000000) & (value >> 8)
                    + (0x0000FF0000000000) & (value >> 24)
                    + (0x00FF000000000000) & (value >> 40)
                    + (0xFF00000000000000) & (value >> 56)));
            }
        }

        private void ReadPlaceableWMFHeader(BinaryReader wmfReader)
        {
            wmfReader.ReadInt16(); // HWmf
            _boundingBox = new Rect(
                new Point(
                    EndianFlip(wmfReader.ReadInt16()),
                    EndianFlip(wmfReader.ReadInt16())),
                new Point(
                    EndianFlip(wmfReader.ReadInt16()),
                    EndianFlip(wmfReader.ReadInt16())
                    )
                );
            _scale = 1440.0 / wmfReader.ReadInt16(); // Inch
            wmfReader.ReadInt32(); // Reserved
            wmfReader.ReadInt16(); // Checksum

            if (_dpiX != 0)
            {
                _currentDC.SetViewportExt(
                    Math.Round(
                        _boundingBox.Width / 1440 * _dpiX * _scale,
                        0,
                        MidpointRounding.ToEven),
                    Math.Round(
                        _boundingBox.Height / 1440 * _dpiY * _scale,
                        0,
                        MidpointRounding.ToEven));
            }
        }

        private static void ReadWMFHeader(BinaryReader wmfReader)
        {
            wmfReader.ReadUInt16(); // Type
            wmfReader.ReadUInt16(); // Header Size
            wmfReader.ReadUInt16(); // Version
            wmfReader.ReadUInt32(); // Size
            wmfReader.ReadUInt16(); // Number of objects
            wmfReader.ReadUInt32(); // Max objects
            wmfReader.ReadUInt16(); // Number of members
        }

        private Record ReadRecord(BinaryReader wmfReader)
        {
            Record newRecord;

            newRecord.Size = EndianFlip(wmfReader.ReadUInt32());
            newRecord.Function = (RecordType)EndianFlip(wmfReader.ReadUInt16());

            return newRecord;
        }

        private void ReadCreateBrushIndirect(BinaryReader wmfReader)
        {
            var wmfBrush = new LogBrush();

            wmfBrush.Style = (BrushStyle)EndianFlip(wmfReader.ReadUInt16());
            var red = wmfReader.ReadByte();
            var green = wmfReader.ReadByte();
            var blue = wmfReader.ReadByte();
            wmfBrush.Colour = Color.FromArgb(255, red, green, blue);
            wmfReader.ReadByte(); // Reserved
            wmfBrush.Hatch = (HatchStyle)EndianFlip(wmfReader.ReadUInt16());

            _currentDC.CreateBrushIndirect(wmfBrush);
        }

        private void ReadCreatePenIndirect(BinaryReader wmfReader, uint recordSize)
        {
            var wmfPen = new LogPen();

            wmfPen.Style = EndianFlip(wmfReader.ReadUInt16());
            wmfPen.Width = EndianFlip(wmfReader.ReadInt16());
            wmfReader.ReadUInt16(); // Not Used
            var red = wmfReader.ReadByte();
            var green = wmfReader.ReadByte();
            var blue = wmfReader.ReadByte();
            wmfReader.ReadByte(); // Reserved
            wmfPen.Colour = Color.FromArgb(255, red, green, blue);

            // Handle non-standard(?) pen record size which has an extra 2 bytes
            if (recordSize == 9)
            {
                wmfReader.ReadUInt16();
            }

            _currentDC.CreatePenIndirect(wmfPen);
        }

        private void ReadCreateDibPatternBrush(BinaryReader wmfReader, uint recordSize)
        {
            var wmfBrush = new LogBrush();
            wmfBrush.Style = (BrushStyle)EndianFlip(wmfReader.ReadUInt16());
            var colorUsage = (ColorUsage)EndianFlip(wmfReader.ReadUInt16());
            int width = 0;
            int height = 0;

            //if (wmfBrush.Style == BrushStyle.BS_PATTERN)
            //{
            //    // TODO: Bitmap16
            //    wmfBrush.Image = ReadBitmap16(wmfReader, (recordSize * 2) - 10);
            //    currentDC.CreateBrushIndirect(wmfBrush);
            //}
            //else 
            //{
            // If style is not BS_PATTERN then BS_DIBPATTERNRT must be assumed
            wmfBrush.Image = ReadDeviceIndependantBitmap(wmfReader, (recordSize * 2) - 10, out width, out height);

            _currentDC.CreateBrushIndirect(wmfBrush);
            //}

        }

        private void ReadCreateFontIndirect(BinaryReader wmfReader, uint recordSize)
        {
            var font = new LogFont();

            font.Height = EndianFlip(wmfReader.ReadInt16());
            font.Width = EndianFlip(wmfReader.ReadInt16());
            font.Escapement = EndianFlip(wmfReader.ReadInt16());
            font.Orientation = EndianFlip(wmfReader.ReadInt16());
            font.Weight = EndianFlip(wmfReader.ReadInt16());
            font.IsItalic = (wmfReader.ReadByte() == 0x01);
            font.IsUnderline = (wmfReader.ReadByte() == 0x01);
            font.IsStrikeout = (wmfReader.ReadByte() == 0x01);
            font.Charset = wmfReader.ReadByte();
            font.OutPrecision = wmfReader.ReadByte();
            font.ClipPrecision = wmfReader.ReadByte();
            font.Quality = wmfReader.ReadByte();
            var pitchAndFamily = wmfReader.ReadByte();
            font.FaceName = AsciiBytesToString(wmfReader.ReadBytes((int)(recordSize * 2) - 6 - 18));
            font.FaceName = font.FaceName.Replace("\0", "").Trim();

            // Split pitch and family
            font.Family = (FamilyFont)((pitchAndFamily & 0x01)
                       + (pitchAndFamily & 0x02)
                       + (pitchAndFamily & 0x03)
                       + (pitchAndFamily & 0x04));
            font.Pitch = (byte)((pitchAndFamily & 0x07)
                       + (pitchAndFamily & 0x08));

            _currentDC.CreateFontIndirect(font);
        }

        private void ReadCreatePalette(BinaryReader wmfReader)
        {
            var palette = new LogPalette();
            var start = wmfReader.ReadUInt16();
            uint itemsNum = wmfReader.ReadUInt16();

            for (int i = 0; i < itemsNum; i++)
            {
                palette.Values.Add((PaletteEntryFlag)wmfReader.ReadByte());
                var blue = wmfReader.ReadByte();
                var green = wmfReader.ReadByte();
                var red = wmfReader.ReadByte();

                palette.Entries.Add(Color.FromArgb(255, red, green, blue));
            }

            _currentDC.CreatePalette(palette);
        }

        // TODO
        private void ReadCreatePatternBrush(BinaryReader wmfReader)
        {
            wmfReader.ReadInt16();  // Type
            var width = EndianFlip(wmfReader.ReadInt16());
            var height = EndianFlip(wmfReader.ReadInt16());
            wmfReader.ReadInt16();  // Width bytes
            wmfReader.ReadByte();  // Planes
            var bitsPixel = wmfReader.ReadByte();
            wmfReader.ReadUInt32();  // Bits
            wmfReader.ReadBytes(18); // Reserved
            wmfReader.ReadBytes((((width * bitsPixel + 15) >> 4) << 1) * height); // Pattern

            _currentDC.CreatePatternBrush(new LogObject());
        }

        private void ReadCreateRegion(BinaryReader wmfReader)
        {
            var points = new List<List<Point>>();

            wmfReader.ReadBytes(2); // nextInChain
            wmfReader.ReadInt16();  // ObjectType
            wmfReader.ReadBytes(4); // ObjectCount
            wmfReader.ReadInt16();  // Region size
            var scanCount = EndianFlip(wmfReader.ReadInt16());  // Scan count
            wmfReader.ReadInt16();  // maxScan
            wmfReader.ReadBytes(8);  // Bounding rectangle

            for (int i = 0; i < scanCount; i++)
            {
                var leftPoints = new List<Point>();
                var rightPoints = new List<Point>();

                ushort coordCount = EndianFlip(wmfReader.ReadUInt16());
                double top = EndianFlip(wmfReader.ReadUInt16());
                double bottom = EndianFlip(wmfReader.ReadUInt16());
                //height = bottom - top;

                for (int c = 0; c < coordCount; c++)
                {
                    leftPoints.Add(
                        new Point(
                            wmfReader.ReadUInt16(),
                            top + c)
                        );

                    rightPoints.Add(
                        new Point(
                            wmfReader.ReadUInt16(),
                            top + c)
                        );
                }

                rightPoints.Reverse();
                leftPoints.AddRange(rightPoints);
                points.Add(leftPoints);

                wmfReader.ReadUInt16(); // Count2
            }

            _currentDC.CreateRegion(new LogRegion(points));
        }

        private void ReadMoveTo(BinaryReader wmfReader)
        {
            double y = EndianFlip(wmfReader.ReadInt16());
            double x = EndianFlip(wmfReader.ReadInt16());

            _currentDC.MoveTo(x, y);
        }

        private void ReadPolygon(BinaryReader wmfReader)
        {
            var vertices = new List<Point>();
            var numPoints = EndianFlip(wmfReader.ReadInt16());

            for (int i = 0; i < numPoints; i++)
            {
                var x = EndianFlip(wmfReader.ReadInt16());
                var y = EndianFlip(wmfReader.ReadInt16());

                vertices.Add(new Point(x, y));
            }

            _currentDC.Polygon(_rootCanvas, vertices);
        }

        private void ReadPolyline(BinaryReader wmfReader)
        {
            var lines = new List<Point>();

            var numPoints = EndianFlip(wmfReader.ReadInt16());

            for (int i = 0; i < numPoints; i++)
            {
                var x = EndianFlip(wmfReader.ReadInt16());
                var y = EndianFlip(wmfReader.ReadInt16());

                lines.Add(new Point(x, y));
            }

            _currentDC.Polyline(_rootCanvas, lines);
        }

        private void ReadPolyPolygon(BinaryReader wmfReader)
        {
            var vertices = new List<IEnumerable<Point>>();

            uint count = EndianFlip(wmfReader.ReadUInt16());
            var polyPoints = new uint[count];

            for (int i = 0; i < count; i++)
            {
                polyPoints[i] = EndianFlip(wmfReader.ReadUInt16());
            }

            for (int i = 0; i < count; i++)
            {
                var points = new List<Point>();

                for (int pointIndex = 0; pointIndex < polyPoints[i]; pointIndex++)
                {
                    var x = EndianFlip(wmfReader.ReadInt16());
                    var y = EndianFlip(wmfReader.ReadInt16());

                    points.Add(new Point(x, y));
                }

                vertices.Add(points);
            }

            _currentDC.PolyPolygon(_rootCanvas, vertices);
        }

        private void ReadDeleteObject(BinaryReader wmfReader)
        {
            _currentDC.DeleteObject(EndianFlip(wmfReader.ReadUInt16()));
        }

        private void ReadSelectClipRegion(BinaryReader wmfReader)
        {
            _currentDC.SelectClipRgn(EndianFlip(wmfReader.ReadUInt16()));
        }

        private void ReadSelectObject(BinaryReader wmfReader)
        {
            _currentDC.SelectObject(EndianFlip(wmfReader.ReadUInt16()));
        }

        private void ReadSelectPalette(BinaryReader wmfReader)
        {
            _currentDC.SelectPalette(EndianFlip(wmfReader.ReadUInt16()));
        }

        private void ReadSetBkColor(BinaryReader wmfReader)
        {
            _currentDC.SetBkColor(Color.FromArgb(
                255,
                wmfReader.ReadByte(),
                wmfReader.ReadByte(),
                wmfReader.ReadByte()));
            wmfReader.ReadByte(); // Reserved
        }

        private void ReadResizePalette(BinaryReader wmfReader)
        {
            _currentDC.ResizePalette(EndianFlip(wmfReader.ReadUInt16()));
        }

        private void ReadRestoreDC(BinaryReader wmfReader)
        {
            _currentDC.RestoreDC(EndianFlip(wmfReader.ReadInt16()));
        }

        private void ReadEscape(BinaryReader wmfReader, uint recordSize)
        {
            var escape = (EscapeFunction)EndianFlip(wmfReader.ReadUInt16());
            uint byteCount = EndianFlip(wmfReader.ReadUInt16()); // unreliable value

            System.Diagnostics.Debug.WriteLine("Escape: " + escape.ToString());

            switch (escape)
            {
                case EscapeFunction.SETMITERLIMIT:
                    _currentDC.SetMiterLimit(EndianFlip(wmfReader.ReadUInt16()));
                    break;

                default:
                    wmfReader.ReadBytes((int)(recordSize * 2) - 10);  // TODO
                    break;
            }
        }

        private void ReadSetTextColor(BinaryReader wmfReader)
        {
            _currentDC.SetTextColor(Color.FromArgb(
                255,
                wmfReader.ReadByte(),
                wmfReader.ReadByte(),
                wmfReader.ReadByte()));
            wmfReader.ReadByte(); // Reserved
        }

        private void ReadRectangle(BinaryReader wmfReader)
        {
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.Rectangle(
                _rootCanvas,
                left,
                top,
                right,
                bottom);
        }

        private void ReadIntersectClipRect(BinaryReader wmfReader)
        {
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.IntersectClipRect(
                left,
                top,
                right,
                bottom);
        }

        private void ReadLineTo(BinaryReader wmfReader)
        {
            double y = EndianFlip(wmfReader.ReadInt16());
            double x = EndianFlip(wmfReader.ReadInt16());

            _currentDC.LineTo(_rootCanvas, x, y);
        }

        private void ReadRoundRectange(BinaryReader wmfReader)
        {
            double height = EndianFlip(wmfReader.ReadInt16());
            double width = EndianFlip(wmfReader.ReadInt16());
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.RoundRect(
                _rootCanvas,
                left,
                top,
                right,
                bottom,
                width,
                height);
        }

        private void ReadDibBitBlt(BinaryReader wmfReader, uint recordSize)
        {
            var rasterOperation = (TernaryRasterOperation)EndianFlip(wmfReader.ReadUInt32());
            var ySrc = EndianFlip(wmfReader.ReadInt16());
            var xSrc = EndianFlip(wmfReader.ReadInt16());
            if (recordSize == 12)
            {
                // Without bitmap reserved field
                wmfReader.ReadInt16();
            }
            short height = EndianFlip(wmfReader.ReadInt16());
            short width = EndianFlip(wmfReader.ReadInt16());
            short yDest = EndianFlip(wmfReader.ReadInt16());
            short xDest = EndianFlip(wmfReader.ReadInt16());

            if (recordSize > 12)
            {
                // With Bitmap
                Image img = ReadDeviceIndependantImage(wmfReader, (recordSize * 2) - 22);

                if (img != null)
                {
                    _currentDC.BitBlt(
                        _rootCanvas,
                        xDest,
                        yDest,
                        width,
                        height,
                        img);
                }
            }
        }

        private void ReadDibStretchBlt(BinaryReader wmfReader, uint recordSize)
        {
            var rasterOperation = (TernaryRasterOperation)EndianFlip(wmfReader.ReadUInt32());
            var srcHeight = EndianFlip(wmfReader.ReadInt16());
            var srcWidth = EndianFlip(wmfReader.ReadInt16());
            var ySrc = EndianFlip(wmfReader.ReadInt16());
            var xSrc = EndianFlip(wmfReader.ReadInt16());
            if (recordSize == 14)
            {
                // Without bitmap reserved field
                wmfReader.ReadInt16();
            }
            var destHeight = EndianFlip(wmfReader.ReadInt16());
            var destWidth = EndianFlip(wmfReader.ReadInt16());
            var yDest = EndianFlip(wmfReader.ReadInt16());
            var xDest = EndianFlip(wmfReader.ReadInt16());

            if (recordSize > 14)
            {
                // With Bitmap
                var img = ReadDeviceIndependantImage(wmfReader, (recordSize * 2) - 26);

                if (img != null)
                {
                    _currentDC.StretchBlt(
                        _rootCanvas,
                        xDest,
                        yDest,
                        destWidth,
                        destHeight,
                        img);
                }
            }
        }

        private void ReadStretchDib(BinaryReader wmfReader, uint recordSize)
        {
            var rasterOperation = (TernaryRasterOperation)EndianFlip(wmfReader.ReadUInt32());
            var colorUsage = (ColorUsage)EndianFlip(wmfReader.ReadUInt16());
            var srcHeight = EndianFlip(wmfReader.ReadInt16());
            var srcWidth = EndianFlip(wmfReader.ReadInt16());
            var ySrc = EndianFlip(wmfReader.ReadInt16());
            var xSrc = EndianFlip(wmfReader.ReadInt16());
            var destHeight = EndianFlip(wmfReader.ReadInt16());
            var destWidth = EndianFlip(wmfReader.ReadInt16());
            var yDest = EndianFlip(wmfReader.ReadInt16());
            var xDest = EndianFlip(wmfReader.ReadInt16());

            var img = ReadDeviceIndependantImage(wmfReader, (recordSize * 2) - 28);

            if (img != null)
            {
                _currentDC.StretchBlt(
                    _rootCanvas,
                    xDest,
                    yDest,
                    destWidth,
                    destHeight,
                    img);
            }
        }

        private void ReadTextOut(BinaryReader wmfReader, uint recordSize)
        {
            var stringLength = EndianFlip(wmfReader.ReadInt16());
            var text = CharsetBytesToString(wmfReader.ReadBytes(stringLength));
            text = text.Replace("\0", "").Trim();
            if (stringLength % 2 != 0)
            {
                wmfReader.ReadByte(); // Padding
            }
            double y = EndianFlip(wmfReader.ReadInt16());
            double x = EndianFlip(wmfReader.ReadInt16());


            _currentDC.TextOut(
                _rootCanvas,
                x,
                y,
                text);
        }

        private Image ReadDeviceIndependantImage(BinaryReader wmfReader, uint dibSize)
        {
            int width = 0;
            int height = 0;

            var source = ReadDeviceIndependantBitmap(wmfReader, dibSize, out width, out height);

            var img = new Image { Width = width, Height = Math.Abs(height), Source = source };

            return img;
        }

        private BitmapSource ReadDeviceIndependantBitmap(BinaryReader wmfReader, uint dibSize, out int width, out int height)
        {
#if NETFX_CORE
            var bmp = new BitmapImage();
            var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var dibBytes = wmfReader.ReadBytes((int)dibSize);
            // int imageBytesOffset = 14;

            //System.Runtime.InteropServices.GCHandle pinnedDib = System.Runtime.InteropServices.GCHandle.Alloc(dibBytes, System.Runtime.InteropServices.GCHandleType.Pinned);

            //if (dibBytes[3] == 0x0C)
            //{
            //    imageBytesOffset += 12;
            //}
            //else
            //{
            //    var infoHeader = (BITMAPINFOHEADER)System.Runtime.InteropServices.Marshal.PtrToStructure(pinnedDib.AddrOfPinnedObject(), typeof(BITMAPINFOHEADER));

            //    imageBytesOffset += (int)infoHeader.biSize;

            //    switch ((BitCount)infoHeader.biBitCount)
            //    {
            //        case BitCount.BI_BITCOUNT_1:
            //            // 1 bit - Two colors
            //            imageBytesOffset += 4 * (colorUsed == 0 ? 2 : Math.Min(colorUsed, 2));
            //            break;

            //        case BitCount.BI_BITCOUNT_2:
            //            // 4 bit - 16 colors
            //            imageBytesOffset += 4 * (colorUsed == 0 ? 16 : Math.Min(colorUsed, 16));
            //            break;

            //        case BitCount.BI_BITCOUNT_3:
            //            // 8 bit - 256 colors
            //            imageBytesOffset += 4 * (colorUsed == 0 ? 256 : Math.Min(colorUsed, 256));
            //            break;
            //    }

            //    if ((Compression)infoHeader.biCompression == Compression.BI_BITFIELDS)
            //    {
            //        imageBytesOffset += 12;
            //    }
            //}

            //pinnedDib.Free();

            using (Windows.Storage.Streams.DataWriter writer = new Windows.Storage.Streams.DataWriter(memStream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(new byte[] { 66, 77 }); // BM
                writer.WriteUInt32(dibSize + 14);
                writer.WriteBytes(new byte[] { 0, 0, 0, 0 }); // Reserved
                writer.WriteUInt32((UInt32)0);

                writer.WriteBytes(dibBytes);
                var t = writer.StoreAsync();
                t.GetResults();
            }

            // bmp.ImageFailed += bmp_ImageFailed;
            // bmp.ImageOpened += bmp_ImageOpened;
            bmp.SetSource(memStream);
            width = bmp.PixelWidth;
            height = bmp.PixelHeight;
            return bmp;
#else
            var bmp = new BitmapImage();
            var memStream = new MemoryStream();
            var dibBytes = wmfReader.ReadBytes((int)dibSize);
            
            BinaryWriter writer = new BinaryWriter(memStream);
            writer.Write(new byte[] { 66, 77 }); // BM
            writer.Write(dibSize + 14);
            writer.Write(new byte[] { 0, 0, 0, 0 }); // Reserved
            writer.Write((UInt32)0);

            writer.Write(dibBytes);
            writer.Flush();

            memStream.Position = 0;
            try
            {
                bmp.BeginInit();
                bmp.StreamSource = memStream;
                bmp.EndInit();
                width = bmp.PixelWidth;
                height = bmp.PixelHeight;

                return bmp;
            }
            catch
            {
                // Bad image;
                width = 0;
                height = 0;

                return null;
            }
#endif
        }
        
        private BitmapSource ReadBitmap16(BinaryReader wmfReader, uint bitmapSize)
        {
            var type = EndianFlip(wmfReader.ReadInt16());
            var width = EndianFlip(wmfReader.ReadInt16());
            var height = EndianFlip(wmfReader.ReadInt16());
            var stride = EndianFlip(wmfReader.ReadInt16());
            var planes = wmfReader.ReadByte();
            var bitsPerPixel = wmfReader.ReadByte();
            //int imageSize = (((width * bitsPerPixel + 15) >> 4) << 1) * height;
            var imageSize = bitmapSize - 10;

            var imgBytes = new byte[(int)imageSize];

            wmfReader.Read(imgBytes, 0, (int)imageSize);

#if NETFX_CORE
            return null;
#else
            var bitmap = BitmapSource.Create(
                width,
                height,
                _dpiX == 0 ? 96 : _dpiX,
                _dpiY == 0 ? 96 : _dpiY,
                PixelFormats.Bgr32,
                null,
                imgBytes,
                stride);

            return bitmap;
#endif
        }

        private static List<Color> ReadPalette(BinaryReader wmfReader, int indexes)
        {
            var colors = new List<Color>();

            for (int i = 0; i < indexes; i++)
            {
                var blue = wmfReader.ReadByte();
                var green = wmfReader.ReadByte();
                var red = wmfReader.ReadByte();

                colors.Add(
                    Color.FromArgb(
                    255,
                    red,
                    green,
                    blue)
                );
                wmfReader.ReadByte(); // Reserved
            }

            return colors;
        }

        private void ReadEllipse(BinaryReader wmfReader)
        {
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.Ellipse(
                _rootCanvas,
                left,
                top,
                right,
                bottom);
        }

        private void ReadArc(BinaryReader wmfReader)
        {
            double endArcY = EndianFlip(wmfReader.ReadInt16());
            double endArcX = EndianFlip(wmfReader.ReadInt16());
            double startArcY = EndianFlip(wmfReader.ReadInt16());
            double startArcX = EndianFlip(wmfReader.ReadInt16());
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.Arc(
                _rootCanvas,
                left,
                top,
                right,
                bottom,
                startArcX,
                startArcY,
                endArcX,
                endArcY);
        }

        private void ReadPie(BinaryReader wmfReader)
        {
            var radial2Y = EndianFlip(wmfReader.ReadInt16());
            var radial2X = EndianFlip(wmfReader.ReadInt16());
            var radial1Y = EndianFlip(wmfReader.ReadInt16());
            var radial1X = EndianFlip(wmfReader.ReadInt16());
            var bottom = EndianFlip(wmfReader.ReadInt16());
            var right = EndianFlip(wmfReader.ReadInt16());
            var top = EndianFlip(wmfReader.ReadInt16());
            var left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.Pie(
                _rootCanvas,
                left,
                top,
                right,
                bottom,
                radial1X,
                radial1Y,
                radial2X,
                radial2Y);
        }

        private void ReadExcludeClipRect(BinaryReader wmfReader)
        {
            double bottom = EndianFlip(wmfReader.ReadInt16());
            double right = EndianFlip(wmfReader.ReadInt16());
            double top = EndianFlip(wmfReader.ReadInt16());
            double left = EndianFlip(wmfReader.ReadInt16());

            _currentDC.ExcludeClipRect(
                left,
                top,
                right,
                bottom);
        }

        private void ReadExtTextOut(BinaryReader wmfReader, uint recordSize)
        {
            Rect dimensions;
            uint expectedSize = 14;
            double[] dx = null;

            double y = EndianFlip(wmfReader.ReadInt16());
            double x = EndianFlip(wmfReader.ReadInt16());
            var stringLength = EndianFlip(wmfReader.ReadInt16());
            expectedSize += (uint)stringLength;
            var fwOpts = (ExtTextOutOptions)EndianFlip(wmfReader.ReadUInt16());

            // Check if we have a rectangle
            if ((fwOpts & ExtTextOutOptions.ETO_CLIPPED) == ExtTextOutOptions.ETO_CLIPPED
                | (fwOpts & ExtTextOutOptions.ETO_OPAQUE) == ExtTextOutOptions.ETO_OPAQUE)
            {
                dimensions = new Rect(
                    new Point(
                        EndianFlip(wmfReader.ReadInt16()),
                        EndianFlip(wmfReader.ReadInt16())
                    ),
                    new Point(
                        EndianFlip(wmfReader.ReadInt16()),
                        EndianFlip(wmfReader.ReadInt16())
                        )
                    );

                expectedSize += 8;
            }
            else
            {
                dimensions = new Rect();
            }

            var text = CharsetBytesToString(wmfReader.ReadBytes(stringLength));
            text = text.Replace("\0", "");
            if (stringLength % 2 != 0)
            {
                wmfReader.ReadByte(); // Padding
                expectedSize += 1;
            }

            // Check if we have a DX
            if (expectedSize < (recordSize * 2))
            {
                dx = new double[stringLength];

                // Handle invalid Illustrator off by 1
                if (expectedSize + (stringLength * 2) != recordSize * 2)
                {
                    stringLength = (short)(((recordSize * 2) - expectedSize) / 2);
                }

                for (int i = 0; i < stringLength; i++)
                {
                    dx[i] = EndianFlip(wmfReader.ReadInt16());
                }
            }

            _currentDC.ExtTextOut(
                _rootCanvas,
                x,
                y,
                fwOpts,
                dimensions,
                text,
                dx);
        }

        private void ReadSaveDC()
        {
            _currentDC.SaveDC();
        }

        private MemoryStream UncompressWmz(MemoryStream input)
        {
            MemoryStream wmfStream;

            input.Position = 0;

            if (input.ReadByte() == 0x1f && input.ReadByte() == 0x8b)
            {
                input.Position = 0;

                wmfStream = new MemoryStream();
                var buffer = new byte[1024];
                var compressedFile = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress, true);
                int bytesRead;

                while ((bytesRead = compressedFile.Read(buffer, 0, buffer.Length)) > 0)
                {
                    wmfStream.Write(buffer, 0, bytesRead);
                }
            }
            else
            {
                wmfStream = input;
            }

            wmfStream.Position = 0;

            return wmfStream;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(this);
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
        public struct BITMAPFILEHEADER
        {
            public ushort bfType;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;
        }
    }
}
