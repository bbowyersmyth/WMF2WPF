using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

#if NETFX_CORE
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Text;
#else
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
#endif

namespace WPFGDI
{
    public class WpfGdi
    {
        private State _currentState;
        private List<State> _stateHistory;
        private ObjectTable _objectTable;
        private double _miterLimit;
#if !NETFX_CORE
        private int _nextImageIndex = 1;
#endif

        public WpfGdi()
        {
            _currentState = new State();
            _stateHistory = new List<State> { _currentState };
            _objectTable = new ObjectTable();
        }


#if !NETFX_CORE
        public string ImageSavePath { get; set; }
#endif

        public void AddObject(LogObject newItem)
        {
            _objectTable.AddObject(newItem);
        }

        public void Arc(Canvas surface, double left, double top, double right, double bottom, double startArcX, double startArcY, double endArcX, double endArcY)
        {
            var arc = new Path();
            var figure = new PathFigure();
            var segments = new PathSegmentCollection();

            var center = new Point(left + ((right - left) / 2), top + ((bottom - top) / 2));
            double degrees = Math.Atan2(startArcY - center.Y, startArcX - center.X)
                            - Math.Atan2(endArcY - center.Y, endArcX - center.X);

            degrees *= 57.2957795; // Convert from radians to degrees
            bool isLargeArc = Math.Abs(degrees) > 180;

            arc.Data = new PathGeometry();

            figure.StartPoint = new Point(LtoDX(startArcX), LtoDY(startArcY));

            var segment = new ArcSegment
                              {
                                  Point = new Point(
                                      LtoDX(endArcX),
                                      LtoDY(endArcY)),
                                  Size = new Size(
                                      (LtoDX(right) - LtoDX(left)) / 2,
                                      (LtoDY(bottom) - LtoDY(top)) / 2),
                                  RotationAngle = 0,
                                  IsLargeArc = isLargeArc,
                                  SweepDirection = SweepDirection.Counterclockwise
                              };

            segments.Add(segment);

            figure.Segments = segments;
            ((PathGeometry)arc.Data).Figures.Add(figure);

            ApplyStyle(arc, false);

            surface.Children.Add(arc);
        }

        public void CreateBrushIndirect(LogBrush brush)
        {
            _objectTable.AddObject(brush);
        }

        public void CreateDibPatternBrush(LogObject brush)
        {
            _objectTable.AddObject(brush);
        }

        public void CreateFontIndirect(LogFont font)
        {
            _objectTable.AddObject(font);
        }

        public void CreatePalette(LogObject palette)
        {
            _objectTable.AddObject(palette);
        }

        public void CreatePatternBrush(LogObject patBrush)
        {
            _objectTable.AddObject(patBrush);
        }

        public void CreatePenIndirect(LogPen pen)
        {
            _objectTable.AddObject(pen);
        }

        public void CreateRegion(LogObject region)
        {
            _objectTable.AddObject(region);
        }

        public void DeleteObject(int index)
        {
            _objectTable.DeleteObject(index);
        }

        public void BitBlt(Canvas surface, double x, double y, double width, double height, Image source)
        {
            double deviceX = LtoDX(x);
            double deviceY = LtoDY(y);
            double deviceWidth = ScaleWidth(width);
            double deviceHeight = ScaleHeight(height);

            Canvas.SetTop(source, deviceY);
            Canvas.SetLeft(source, deviceX);
            source.Width = deviceWidth;
            source.Height = deviceHeight;

#if !NETFX_CORE
            var clipGeometry = GetClipGeometryAsChild(
                            new Rect(
                                deviceX,
                                deviceY,
                                deviceWidth,
                                deviceHeight)
                            );

            if (clipGeometry != null)
            {
                source.Clip = clipGeometry;
            }

            if (!String.IsNullOrEmpty(ImageSavePath))
            {
                SaveImage(source);
            }
#endif

            surface.Children.Add(source);
        }

        public void Ellipse(Canvas surface, double left, double top, double right, double bottom)
        {
            var ell = new Ellipse();

            Canvas.SetTop(ell, LtoDY(top));
            Canvas.SetLeft(ell, LtoDX(left));
            ell.Width = Math.Abs(LtoDX(right) - LtoDX(left));
            ell.Height = Math.Abs(LtoDY(bottom) - LtoDY(top));

            ApplyStyle(ell, true);

            surface.Children.Add(ell);
        }

        public void ExcludeClipRect(double left, double top, double right, double bottom)
        {
            var clipRegion = _currentState.CurrentClipRegion;
            var clipRect = new RectangleGeometry
                {
                    Rect = new Rect(
                            new Point(
                                LtoDX(left),
                                LtoDY(top)),
                            new Point(
                                LtoDX(right),
                                LtoDY(bottom))
                            )
                };

            if (clipRegion == null)
            {
                clipRegion = GetDefaultClipRegion();
            }

#if !NETFX_CORE
            _currentState.CurrentClipRegion = new LogRegion(
                    Geometry.Combine(
                        clipRegion.Geometry,
                        clipRect,
                        GeometryCombineMode.Exclude,
                        null)
                );
#endif
        }

        public void ExtTextOut(Canvas surface, double x, double y, ExtTextOutOptions options, Rect dimensions, string text, double[] dx)
        {
            TextBlock block = null;
            Grid container = null;
            double deviceX = LtoDX(x);
            double deviceY = LtoDY(y);
            double textWidth;
            double textHeight;
            Geometry clipGeometry = null;

            if (dx == null)
            {
                // No character placement
                block = new TextBlock
                    {
                        Inlines = { 
                            new Run
                                {
                                    Text = text
                                }
                        }
                    };

                ApplyTextStyle(block, deviceY, true, options);

                //Canvas.SetLeft(block, deviceX);

                surface.Children.Add(block);
                block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textHeight = block.DesiredSize.Height;
                textWidth = block.DesiredSize.Width;
            }
            else
            {
                // Place individual characters
                int stringLength = text.Length;
                double currentPos = 0; // LtoDX(x);
                container = new Grid();

                for (int i = 0; i < stringLength; i++)
                {
                    block = new TextBlock
                        {
                            Inlines = { 
                                new Run 
                                {
                                    Text = text.Substring(i, 1)
                                }
                            }
                        };
                    //TextOptions.SetTextFormattingMode(block, TextFormattingMode.Display);
                    ApplyTextStyle(block, deviceY, false, null);

                    if (i > 0)
                    {
                        currentPos += ScaleWidth(dx[i - 1]);
                    }
                    block.Margin = new Thickness(currentPos, 0, 0, 0);
                    // TODO: Character width
                    //block.Width = dx[i];

                    container.Children.Add(block);
                }

                //Canvas.SetLeft(container, deviceX);
                ApplyTextContainerStyle(container, deviceY);

                surface.Children.Add(container);
                container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textHeight = container.DesiredSize.Height;
                textWidth = container.DesiredSize.Width;
            }

            // Horizontal alignment
            if ((_currentState.TextAlign & (ushort)TextAlignmentMode.TA_CENTER) == (ushort)TextAlignmentMode.TA_CENTER)
            {
                deviceX -= textWidth / 2.0;
            }
            else if ((_currentState.TextAlign & (ushort)TextAlignmentMode.TA_RIGHT) == (ushort)TextAlignmentMode.TA_RIGHT)
            {
                deviceX -= textWidth;
            }

            // Vertical Alignment
            if ((_currentState.TextAlign & (ushort)TextAlignmentMode.TA_BASELINE) == (ushort)TextAlignmentMode.TA_BASELINE)
            {
                double baseline;

#if NETFX_CORE
                baseline = textHeight;
#else
                if (block != null)
                {
                    var formattedText = new FormattedText(
                         text,
                         System.Globalization.CultureInfo.CurrentCulture,
                         block.FlowDirection,
                         new Typeface(block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch, block.FontFamily),
                         block.FontSize,
                         block.Foreground);

                    baseline = formattedText.Baseline;
                }
                else
                {
                    baseline = textHeight;
                }
#endif

                deviceY -= baseline;
            }
            else if ((_currentState.TextAlign & (ushort)TextAlignmentMode.TA_BOTTOM) == (ushort)TextAlignmentMode.TA_BOTTOM)
            {
                deviceY -= textHeight;
            }

            // Clip
            clipGeometry = GetClipGeometryAsChild(
                new Rect(
                    deviceX,
                    deviceY,
                    textWidth,
                    textHeight)
                );

            if (container == null)
            {
                Canvas.SetLeft(block, deviceX);
                Canvas.SetTop(block, deviceY);

#if !NETFX_CORE
                if (clipGeometry != null)
                {
                    block.Clip = clipGeometry;
                }
#endif
            }
            else
            {
                Canvas.SetLeft(container, deviceX);
                Canvas.SetTop(container, deviceY);

#if !NETFX_CORE
                if (clipGeometry != null)
                {
                    container.Clip = clipGeometry;
                }
#endif
            }

            if ((options & ExtTextOutOptions.ETO_OPAQUE) == ExtTextOutOptions.ETO_OPAQUE)
            {
                block.Height = ScaleHeight(dimensions.Height);
                block.Width = ScaleWidth(dimensions.Width);
            }

            //if ((currentState.TextAlign & (ushort)TextAlignmentMode.TA_UPDATECP) == (ushort)TextAlignmentMode.TA_UPDATECP)
            //{
            //    // Update current pos
            //    Point pos = currentState.CurrentPosition;


            //    // TODO: Mapx and y
            //    pos.X = Canvas.GetLeft(block) + block.ActualWidth;
            //    pos.Y = Canvas.GetTop(block) + block.ActualHeight;

            //    currentState.CurrentPosition = pos;
            //}
        }

        public List<Color> GetPaletteEntries(int startIndex, int count)
        {
            if (_currentState.CurrentPalette == null)
            {
                return Palettes.Halftone256.Skip(startIndex).Take(count).ToList();
            }
            else
            {
                return _currentState.CurrentPalette.Entries.GetRange(startIndex, count);
            }
        }

        public Point GetWindowOrg()
        {
            return _currentState.WindowOrigin;
        }

        public void IntersectClipRect(double left, double top, double right, double bottom)
        {
            LogRegion clipRegion = _currentState.CurrentClipRegion;
            var clipRect = new RectangleGeometry
                {
                    Rect = new Rect(
                        new Point(
                            LtoDX(left),
                            LtoDY(top)),
                        new Point(
                            LtoDX(right),
                            LtoDY(bottom))
                        )
                };

            if (clipRegion == null)
            {
                _currentState.CurrentClipRegion = new LogRegion(clipRect);
            }
            else
            {
#if !NETFX_CORE
                _currentState.CurrentClipRegion = new LogRegion(
                        Geometry.Combine(
                            clipRegion.Geometry,
                            clipRect,
                            GeometryCombineMode.Intersect,
                            null)
                    );
#endif
            }
        }

        public void LineTo(Canvas surface, double x, double y)
        {
            var line = new Path();
            var curPos = _currentState.CurrentPosition;
            var startPos = new Point(LtoDX(curPos.X), LtoDY(curPos.Y));
            var endPos = new Point(LtoDX(x), LtoDY(y));

#if NETFX_CORE
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            geometry.Figures.Add(figure);
            figure.StartPoint = startPos;
            figure.Segments.Add(
                new LineSegment()
                {
                    Point = endPos
                }
            );
#else
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(startPos, false, false);
                ctx.LineTo(endPos, true, false);
            }

            geometry.Freeze();
#endif

            line.Data = geometry;
#if !NETFX_CORE
            line.SnapsToDevicePixels = true;
#endif

            ApplyStyle(line, false);

            surface.Children.Add(line);

            _currentState.CurrentPosition = new Point(x, y);
        }

        public void MoveTo(double x, double y)
        {
            _currentState.CurrentPosition = new Point(x, y);
        }

        public void Pie(Canvas surface, double left, double top, double right, double bottom, double radial1X, double radial1Y, double radial2X, double radial2Y)
        {
            var pie = new Path();
            var figure = new PathFigure();
            var segments = new PathSegmentCollection();

            var center = new Point(left + ((right - left) / 2), top + ((bottom - top) / 2));
            var degrees = Math.Atan2(radial1Y - center.Y, radial1X - center.X)
                            - Math.Atan2(radial2Y - center.Y, radial2X - center.X);

            degrees *= 57.2957795; // Convert from radians to degrees
            bool isLargeArc = Math.Abs(degrees) > 180;

            pie.Data = new PathGeometry();

            figure.IsClosed = true;
            figure.StartPoint = new Point(LtoDX(radial1X), LtoDY(radial1Y));
            var segment = new ArcSegment
                {
                    Point = new Point(LtoDX(radial1X),
                              LtoDY(radial1Y)),
                    Size = new Size((LtoDX(right) - LtoDX(left)) / 2,
                           (LtoDY(bottom) - LtoDY(top)) / 2),
                    RotationAngle = 0,
                    IsLargeArc = isLargeArc,
                    SweepDirection = SweepDirection.Counterclockwise,
                };

            segments.Add(segment);
            segments.Add(
                new LineSegment
                    {
                        Point = center
                    }
                );

            figure.Segments = segments;
            ((PathGeometry)pie.Data).Figures.Add(figure);

            ApplyStyle(pie, true);

            surface.Children.Add(pie);
        }

        public void Polygon(Canvas surface, IEnumerable<Point> vertices)
        {
            var polyPath = new Path();
            var i = 0;

#if NETFX_CORE
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            geometry.Figures.Add(figure);

            foreach (var item in vertices)
            {
                if (i == 0)
                {
                    // Move to
                    figure.IsFilled = true;
                    figure.IsClosed = true;
                    figure.StartPoint = new Point(LtoDX(item.X), LtoDY(item.Y));
                }
                else
                {
                    figure.Segments.Add(
                        new LineSegment()
                        {
                            Point = new Point(LtoDX(item.X), LtoDY(item.Y))
                        }
                    );
                }

                i++;
            }
#else
            var geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                foreach (var item in vertices)
                {
                    if (i == 0)
                    {
                        // Move to
                        ctx.BeginFigure(new Point(LtoDX(item.X), LtoDY(item.Y)), true, true);
                    }
                    else
                    {
                        ctx.LineTo(new Point(LtoDX(item.X), LtoDY(item.Y)), true, false);
                    }

                    i++;
                }
            }
#endif

            if (_currentState.PolyFillMode == PolyFillMode.WINDING)
            {
                geometry.FillRule = FillRule.Nonzero;
            }


#if !NETFX_CORE
            geometry.Freeze();
#endif

            polyPath.Data = geometry;


#if !NETFX_CORE
            // Clip
            var clipGeometry = GetClipGeometry(geometry.Bounds);

            if (clipGeometry != null)
            {
                polyPath.Data = Geometry.Combine(polyPath.Data, clipGeometry, GeometryCombineMode.Intersect, null);
            }
#endif

            ApplyStyle(polyPath, true);

            surface.Children.Add(polyPath);
        }

        public void Polyline(Canvas surface, IEnumerable<Point> lines)
        {
            var polyPath = new Path();
            var i = 0;

#if NETFX_CORE
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            geometry.Figures.Add(figure);

            foreach (var item in lines)
            {
                if (i == 0)
                {
                    // Move to
                    figure.IsFilled = true;
                    figure.IsClosed = false;
                    figure.StartPoint = new Point(LtoDX(item.X), LtoDY(item.Y));
                }
                else
                {
                    figure.Segments.Add(
                        new LineSegment()
                        {
                            Point = new Point(LtoDX(item.X), LtoDY(item.Y))
                        }
                    );
                }

                i++;
            }
#else
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                foreach (var item in lines)
                {
                    if (i == 0)
                    {
                        // Move to
                        ctx.BeginFigure(new Point(LtoDX(item.X), LtoDY(item.Y)), true, false);
                    }
                    else
                    {
                        ctx.LineTo(new Point(LtoDX(item.X), LtoDY(item.Y)), true, false);
                    }

                    i++;
                }
            }
#endif

            if (_currentState.PolyFillMode == PolyFillMode.WINDING)
            {
                geometry.FillRule = FillRule.Nonzero;
            }

#if !NETFX_CORE
            geometry.Freeze();
#endif

            polyPath.Data = geometry;


#if !NETFX_CORE
            // Clip
            var clipGeometry = GetClipGeometry(geometry.Bounds);

            if (clipGeometry != null)
            {
                polyPath.Data = Geometry.Combine(polyPath.Data, clipGeometry, GeometryCombineMode.Intersect, null);
            }
#endif

            ApplyStyle(polyPath, false);

            surface.Children.Add(polyPath);
        }

        public void PolyPolygon(Canvas surface, IEnumerable<IEnumerable<Point>> vertices)
        {
            var polyPath = new Path();

#if NETFX_CORE
            var geometry = new PathGeometry();

            foreach (var polygon in vertices)
            {
                var pointIndex = 0;
                var figure = new PathFigure();

                geometry.Figures.Add(figure);

                foreach (Point item in polygon)
                {
                    if (pointIndex == 0)
                    {
                        // Move to
                        figure.IsFilled = true;
                        figure.IsClosed = true;
                        figure.StartPoint = new Point(LtoDX(item.X), LtoDY(item.Y));
                    }
                    else
                    {
                        figure.Segments.Add(
                            new LineSegment()
                            {
                                Point = new Point(LtoDX(item.X), LtoDY(item.Y))
                            }
                        );
                    }

                    pointIndex++;
                }
            }
#else
            var geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                foreach (var polygon in vertices)
                {
                    var pointIndex = 0;

                    foreach (Point item in polygon)
                    {
                        if (pointIndex == 0)
                        {
                            // Move to
                            ctx.BeginFigure(new Point(LtoDX(item.X), LtoDY(item.Y)), true, true);
                        }
                        else
                        {
                            ctx.LineTo(new Point(LtoDX(item.X), LtoDY(item.Y)), true, false);
                        }

                        pointIndex++;
                    }
                }
            }
#endif

            if (_currentState.PolyFillMode == PolyFillMode.WINDING)
            {
                geometry.FillRule = FillRule.Nonzero;
            }

#if !NETFX_CORE
            geometry.Freeze();
#endif

            polyPath.Data = geometry;

#if !NETFX_CORE
            // Clip
            var clipGeometry = GetClipGeometry(geometry.Bounds);

            if (clipGeometry != null)
            {
                polyPath.Data = Geometry.Combine(polyPath.Data, clipGeometry, GeometryCombineMode.Intersect, null);
            }
#endif

            ApplyStyle(polyPath, true);

            surface.Children.Add(polyPath);
        }

        public void RealizePalette()
        {
            if (_currentState.CurrentPalette != null)
            {
                // TODO: Handle palette values
                // currentState.RealizedPalette = new BitmapPalette(currentState.CurrentPalette.Entries);
            }
        }

        public void Rectangle(Canvas surface, double left, double top, double right, double bottom)
        {
            var rec = new Rectangle();

            Canvas.SetTop(rec, LtoDY(top));
            Canvas.SetLeft(rec, LtoDX(left));
            rec.Width = Math.Abs(LtoDX(right) - LtoDX(left));
            rec.Height = Math.Abs(LtoDY(bottom) - LtoDY(top));

            ApplyStyle(rec, true);

            surface.Children.Add(rec);
        }

        public void ResizePalette(ushort count)
        {
            if (_currentState.CurrentPalette != null && _currentState.CurrentPalette.Entries.Count - 1 > count)
            {
                // TODO: Confirm functionality
                for (int i = _currentState.CurrentPalette.Entries.Count - 1; i > count; i--)
                {
                    _currentState.CurrentPalette.Entries.RemoveAt(i);
                }
            }
        }

        public void RestoreDC(int index)
        {
            if (index < 0)
            {
                // Relative restore
                index = _stateHistory.Count - 1 + index;
            }

            _currentState = _stateHistory[index];

            // Discard any newer DC's
            for (int i = _stateHistory.Count - 1; i > index; i--)
            {
                _stateHistory.RemoveAt(i);
            }
        }

        public void RoundRect(Canvas surface, double left, double top, double right, double bottom, double width, double height)
        {
            var rec = new Rectangle();

            Canvas.SetTop(rec, LtoDY(top));
            Canvas.SetLeft(rec, LtoDX(left));
            rec.Width = Math.Abs(LtoDX(right) - LtoDX(left));
            rec.Height = Math.Abs(LtoDY(bottom) - LtoDY(top));
            rec.RadiusX = ScaleWidth(width) / 2;
            rec.RadiusY = ScaleHeight(height) / 2;

            ApplyStyle(rec, true);

            surface.Children.Add(rec);
        }

        public void SaveDC()
        {
            _currentState = _currentState.Clone();
            _stateHistory.Add(_currentState);
        }

        public void SelectClipRgn(int index)
        {
            if (_objectTable.Items[index] is LogRegion)
            {
                _currentState.CurrentClipRegion = (LogRegion)_objectTable.Items[index];
            }
            else
            {
                _currentState.CurrentClipRegion = null;
            }
        }

        public LogObject SelectObject(int index)
        {
            if (_objectTable.Items[index] is LogPen)
            {
                _currentState.CurrentPen = (LogPen)_objectTable.Items[index];
            }
            else if (_objectTable.Items[index] is LogBrush)
            {
                _currentState.CurrentBrush = (LogBrush)_objectTable.Items[index];
            }
            else if (_objectTable.Items[index] is LogFont)
            {
                _currentState.CurrentFont = (LogFont)_objectTable.Items[index];
            }

            return _objectTable.Items[index];
        }

        public LogObject SelectPalette(ushort index)
        {
            if (_objectTable.Items[index] is LogPalette)
            {
                _currentState.CurrentPalette = (LogPalette)_objectTable.Items[index];
            }

            return _objectTable.Items[index];
        }

        public void SetBkColor(Color bkColor)
        {
            _currentState.BackgroundColour = bkColor;
        }

        public void SetBkMode(MixMode bkMode)
        {
            _currentState.BackgroundMode = bkMode;
        }

        public void SetMapMode(MapMode mapMode)
        {
            _currentState.MapMode = mapMode;
        }

        public void SetMiterLimit(double limit)
        {
            _miterLimit = limit;
        }

        public void SetPolyFillMode(PolyFillMode polyFillMode)
        {
            _currentState.PolyFillMode = polyFillMode;
        }

        public void SetRop2(BinaryRasterOperation drawMode)
        {
            _currentState.DrawMode = drawMode;
        }

        public void SetStretchBltMode(StretchMode stretchMode)
        {
            _currentState.StretchBltMode = stretchMode;
        }

        public void SetTextAlign(ushort textAlign)
        {
            _currentState.TextAlign = textAlign;
        }

        public void SetTextColor(Color textColor)
        {
            _currentState.TextColor = textColor;
        }

        public void SetViewportExt(double x, double y)
        {
            _currentState.ViewportExtX = x;
            _currentState.ViewportExtY = y;
        }

        public void SetViewportOrg(double x, double y)
        {
            _currentState.ViewportOrigin = new Point(x, y);
        }

        public void SetWindowExt(double x, double y)
        {
            _currentState.WindowExtX = x;
            _currentState.WindowExtY = y;
        }

        public Size GetWindowExt()
        {
            return new Size(_currentState.WindowExtX, _currentState.WindowExtY);
        }

        public void SetWindowOrg(double x, double y)
        {
            _currentState.WindowOrigin = new Point(x, y);
        }

        public void StretchBlt(Canvas surface, double x, double y, double width, double height, Image source)
        {
            var deviceX = LtoDX(x);
            var deviceY = LtoDY(y);
            var deviceWidth = ScaleWidth(width);
            var deviceHeight = ScaleHeight(height);

            Canvas.SetTop(source, deviceY);
            Canvas.SetLeft(source, deviceX);
            source.Width = deviceWidth;
            source.Height = deviceHeight;
            source.Stretch = Stretch.Fill;

#if !NETFX_CORE
            var clipGeometry = GetClipGeometryAsChild(
                new Rect(
                    deviceX,
                    deviceY,
                    deviceWidth,
                    deviceHeight)
                );

            if (clipGeometry != null)
            {
                source.Clip = clipGeometry;
            }

            if (!String.IsNullOrEmpty(ImageSavePath))
            {
                SaveImage(source);
            }
#endif

            surface.Children.Add(source);
        }

        public void TextOut(Canvas surface, double x, double y, string text)
        {
            //TextBlock block;

            //block = new TextBlock(new System.Windows.Documents.Run(text));

            //ApplyTextStyle(block, LtoDY(y), true);

            //Canvas.SetLeft(block, LtoDX(x));

            //if ((currentState.TextAlign & (ushort)TextAlignmentMode.TA_UPDATECP) == (ushort)TextAlignmentMode.TA_UPDATECP)
            //{
            //    // Update current pos
            //    Point pos = currentState.CurrentPosition;


            //    // TODO: Mapx and y
            //    pos.X = Canvas.GetLeft(block) + block.ActualWidth;
            //    pos.Y = Canvas.GetTop(block) + block.ActualHeight;

            //    currentState.CurrentPosition = pos;
            //}

            //surface.Children.Add(block);

            ExtTextOut(surface, x, y, ExtTextOutOptions.ETO_NO_RECT, Rect.Empty, text, null);
        }

        private void ApplyStyle(Shape shape, bool fillShape)
        {
            if (_currentState.CurrentPen == null)
            {
                // Stock Pen
                var newBrush = new SolidColorBrush(Colors.Black);
#if !NETFX_CORE
                newBrush.Freeze();
#endif
                shape.Stroke = newBrush;
                shape.StrokeThickness = 1;
            }
            else
            {
                LogPen currentPen = _currentState.CurrentPen;

                if (currentPen.Width > 0)
                {
                    shape.StrokeThickness = ScaleWidth(currentPen.Width);
                }


                // Style
                if ((PenStyle)(currentPen.Style & 0x000F) == PenStyle.PS_NULL)
                {
                    // Do nothing, null is the default
                    //shape.Stroke = null;
                }
                else
                {
                    var newBrush = new SolidColorBrush(currentPen.Colour);
#if !NETFX_CORE
                    newBrush.Freeze();
#endif
                    shape.Stroke = newBrush;

                    if ((PenStyle)(currentPen.Style & 0x000F) == PenStyle.PS_DASH)
                    {
                        shape.StrokeDashArray.Add(30);
                        shape.StrokeDashArray.Add(10);
                    }
                    else if ((PenStyle)(currentPen.Style & 0x000F) == PenStyle.PS_DASHDOT)
                    {
                        shape.StrokeDashArray.Add(30);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                    }
                    else if ((PenStyle)(currentPen.Style & 0x000F) == PenStyle.PS_DASHDOTDOT)
                    {

                        shape.StrokeDashArray.Add(30);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                    }
                    else if ((PenStyle)(currentPen.Style & 0x000F) == PenStyle.PS_DOT)
                    {
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashArray.Add(10);
                        shape.StrokeDashCap = PenLineCap.Round;
                    }
                }

                // Join
                if ((PenStyle)(currentPen.Style & 0xF000) == PenStyle.PS_JOIN_BEVEL)
                {
                    shape.StrokeLineJoin = PenLineJoin.Bevel;
                }
                else if ((PenStyle)(currentPen.Style & 0xF000) == PenStyle.PS_JOIN_MITER)
                {
                    // Do nothing, miter is the default
                    shape.StrokeLineJoin = PenLineJoin.Miter;

                    if (_miterLimit != 0)
                    {
                        shape.StrokeMiterLimit = _miterLimit;
                    }
                }
                else if ((PenStyle)(currentPen.Style & 0xF000) == PenStyle.PS_JOIN_ROUND)
                {
                    shape.StrokeLineJoin = PenLineJoin.Round;
                }

                // End cap
                if ((PenStyle)(currentPen.Style & 0x0F00) == PenStyle.PS_ENDCAP_FLAT)
                {
                    // Do nothing, flat is the default
                    // shape.StrokeEndLineCap = PenLineCap.Flat;
                    // shape.StrokeStartLineCap = PenLineCap.Flat;
                }
                else if ((PenStyle)(currentPen.Style & 0x0F00) == PenStyle.PS_ENDCAP_SQUARE)
                {
                    shape.StrokeEndLineCap = PenLineCap.Square;
                    shape.StrokeStartLineCap = PenLineCap.Square;
                }
                else if ((PenStyle)(currentPen.Style & 0x0F00) == PenStyle.PS_ENDCAP_ROUND)
                {
                    shape.StrokeEndLineCap = PenLineCap.Round;
                    shape.StrokeStartLineCap = PenLineCap.Round;
                }
            }

            if (_currentState.CurrentBrush == null & fillShape)
            {
                // Stock brush
                var newBrush = new SolidColorBrush(Colors.White);
#if !NETFX_CORE
                newBrush.Freeze();
#endif
                shape.Fill = newBrush;
            }
            else if (fillShape)
            {
                LogBrush currentBrush = _currentState.CurrentBrush;

                if (currentBrush.Style == BrushStyle.BS_NULL)
                {
                    // Do nothing, null is the default
                    // shape.Fill = null;
                }
                else if (currentBrush.Image != null)
                {
#if !NETFX_CORE
                    var imgBrush = new ImageBrush
                                       {
                                           ImageSource = currentBrush.Image,
                                           Stretch = Stretch.None,
                                           TileMode = TileMode.Tile,
                                           Viewport =
                                               new Rect(0, 0, ScaleWidth(currentBrush.Image.Width),
                                                        ScaleHeight(currentBrush.Image.Height)),
                                           ViewportUnits = BrushMappingMode.Absolute,
                                           Viewbox =
                                               new Rect(0, 0, ScaleWidth(currentBrush.Image.Width),
                                                        ScaleHeight(currentBrush.Image.Height)),
                                           ViewboxUnits = BrushMappingMode.Absolute
                                       };

                    imgBrush.Freeze();

                    shape.Fill = imgBrush;
#endif
                    // TODO: Figure out a way to stop the tile anti-aliasing
                }
                else if (currentBrush.Style == BrushStyle.BS_PATTERN
                    | currentBrush.Style == BrushStyle.BS_DIBPATTERN
                    | currentBrush.Style == BrushStyle.BS_DIBPATTERNPT)
                {
                    var newBrush = new SolidColorBrush(Colors.Black);
#if !NETFX_CORE
                    newBrush.Freeze();
#endif
                    shape.Fill = newBrush;
                }
                else if (currentBrush.Style == BrushStyle.BS_HATCHED)
                {
#if !NETFX_CORE
                    var patternBrush = new VisualBrush();
                    var patternTile = new Canvas();
                    var stroke1 = new Path();
                    var stroke2 = new Path();
                    var figure1 = new PathFigure();
                    var figure2 = new PathFigure();
                    var segments1 = new PathSegmentCollection();
                    var segments2 = new PathSegmentCollection();

                    patternBrush.TileMode = TileMode.Tile;
                    patternBrush.Viewport = new Rect(0, 0, 10, 10);
                    patternBrush.ViewportUnits = BrushMappingMode.Absolute;
                    patternBrush.Viewbox = new Rect(0, 0, 10, 10);
                    patternBrush.ViewboxUnits = BrushMappingMode.Absolute;

                    stroke1.Data = new PathGeometry();
                    stroke2.Data = new PathGeometry();

                    switch (currentBrush.Hatch)
                    {
                        case HatchStyle.HS_BDIAGONAL:
                            // A 45-degree upward, left-to-right hatch.
                            figure1.StartPoint = new Point(0, 10);

                            var up45Segment = new LineSegment
                                {
                                    Point = new Point(10, 0)
                                };
                            segments1.Add(up45Segment);
                            figure1.Segments = segments1;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            patternTile.Children.Add(stroke1);
                            break;

                        case HatchStyle.HS_CROSS:
                            // A horizontal and vertical cross-hatch.
                            figure1.StartPoint = new Point(5, 0);
                            figure2.StartPoint = new Point(0, 5);

                            var downXSegment = new LineSegment
                                {
                                    Point = new Point(5, 10)
                                };
                            segments1.Add(downXSegment);
                            figure1.Segments = segments1;
                            var rightXSegment = new LineSegment
                                {
                                    Point = new Point(10, 5)
                                };
                            segments2.Add(rightXSegment);
                            figure1.Segments = segments1;
                            figure2.Segments = segments2;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            ((PathGeometry)stroke2.Data).Figures.Add(figure2);

                            patternTile.Children.Add(stroke1);
                            patternTile.Children.Add(stroke2);
                            break;

                        case HatchStyle.HS_DIAGCROSS:
                            // A 45-degree crosshatch.
                            figure1.StartPoint = new Point(0, 0);
                            figure2.StartPoint = new Point(0, 10);

                            var downDXSegment = new LineSegment
                                {
                                    Point = new Point(10, 10)
                                };
                            segments1.Add(downDXSegment);
                            figure1.Segments = segments1;
                            var rightDXSegment = new LineSegment
                                {
                                    Point = new Point(10, 0)
                                };
                            segments2.Add(rightDXSegment);
                            figure1.Segments = segments1;
                            figure2.Segments = segments2;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            ((PathGeometry)stroke2.Data).Figures.Add(figure2);

                            patternTile.Children.Add(stroke1);
                            patternTile.Children.Add(stroke2);
                            break;

                        case HatchStyle.HS_FDIAGONAL:
                            // A 45-degree downward, left-to-right hatch.
                            figure1.StartPoint = new Point(0, 0);

                            var down45Segment = new LineSegment
                                {
                                    Point = new Point(10, 10)
                                };
                            segments1.Add(down45Segment);
                            figure1.Segments = segments1;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            patternTile.Children.Add(stroke1);
                            break;

                        case HatchStyle.HS_HORIZONTAL:
                            // A horizontal hatch.
                            figure1.StartPoint = new Point(0, 10);

                            var bottomSegment = new LineSegment
                                {
                                    Point = new Point(10, 10)
                                };
                            segments1.Add(bottomSegment);
                            figure1.Segments = segments1;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            patternTile.Children.Add(stroke1);
                            break;

                        case HatchStyle.HS_VERTICAL:
                            // A vertical hatch.
                            figure1.StartPoint = new Point(10, 0);

                            var verticalSegment = new LineSegment
                                {
                                    Point = new Point(10, 10)
                                };
                            segments1.Add(verticalSegment);
                            figure1.Segments = segments1;

                            ((PathGeometry)stroke1.Data).Figures.Add(figure1);
                            patternTile.Children.Add(stroke1);
                            break;
                    }

                    patternBrush.Visual = patternTile;
                    //patternBrush.Freeze();  // Cant freeze a visual brush
                    shape.Fill = patternBrush;
#endif
                }
                else
                {
                    var newBrush = new SolidColorBrush(currentBrush.Colour);
#if !NETFX_CORE
                    newBrush.Freeze();
#endif
                    shape.Fill = newBrush;
                }
            }
        }

        private void ApplyTextStyle(TextBlock block, double y, bool applyTransforms, ExtTextOutOptions? options)
        {
            block.Foreground = new SolidColorBrush(_currentState.TextColor);
            if (_currentState.BackgroundMode == MixMode.OPAQUE ||
                (options.HasValue && (options.Value & ExtTextOutOptions.ETO_OPAQUE) == ExtTextOutOptions.ETO_OPAQUE))
            {
#if !NETFX_CORE
                block.Background = new SolidColorBrush(_currentState.BackgroundColour);
#endif
            }

            var font = _currentState.CurrentFont;

            if (font != null)
            {
                block.FontFamily = new FontFamily(font.FaceName);
                block.FontSize = 12; // TODO
                //block.FontStretch = FontStretches.UltraExpanded; // TODO
                if (font.Height != 0)
                {
                    block.FontSize = Math.Abs(ScaleHeight(font.Height));
                }

                if (font.IsItalic)
                {
#if NETFX_CORE
                    block.FontStyle = FontStyle.Italic;
#else
                    block.FontStyle = FontStyles.Italic;
#endif
                }

#if !NETFX_CORE
                if (font.IsStrikeout)
                {
                    block.TextDecorations.Add(TextDecorations.Strikethrough);
                }

                if (font.IsUnderline)
                {
                    block.TextDecorations.Add(TextDecorations.Underline);
                }
#endif

                if (font.Weight > 0)
                {
#if NETFX_CORE
                    block.FontWeight = new FontWeight() { Weight = (ushort)font.Weight };
#else
                    block.FontWeight = FontWeight.FromOpenTypeWeight(font.Weight);
#endif
                }

                if (applyTransforms)
                {
                    Canvas.SetTop(block, y - block.FontSize);

                    // Rotation
                    if (font.Escapement != 0)
                    {
                        var rotate = new RotateTransform
                            {
                                Angle = -1 * font.Escapement / 10.0
#if !NETFX_CORE
                                , CenterY = block.FontSize * block.FontFamily.Baseline
#endif
                            };
                        block.RenderTransform = rotate;
                    }
                }
            }
        }

        private void ApplyTextContainerStyle(Panel container, double y)
        {
            var font = _currentState.CurrentFont;

            if (font != null && container.Children.Count > 0)
            {
                Canvas.SetTop(container, y - ((TextBlock)container.Children[0]).FontSize);

                // Rotation
                if (font.Escapement != 0)
                {
                    var block = ((TextBlock)container.Children[0]);
                    var rotate = new RotateTransform
                                     {
                                         Angle = -1 * font.Escapement / 10.0
#if !NETFX_CORE
                                         , CenterY = block.FontSize * block.FontFamily.Baseline
#endif
                                     };
                    container.RenderTransform = rotate;
                }
            }
        }

        public double LtoDY(double y)
        {
            if (_currentState.ViewportExtY == 0)
            {
                return Math.Abs(y - _currentState.WindowOrigin.Y);
            }
            else
            {
                return Math.Round(
                    (y - _currentState.WindowOrigin.Y)
                    * (_currentState.ViewportExtY / _currentState.WindowExtY)
                    + _currentState.ViewportOrigin.Y,
                    0,
                    MidpointRounding.ToEven);
            }
        }

        public double LtoDX(double x)
        {
            if (_currentState.ViewportExtX == 0)
            {
                return Math.Abs(x - _currentState.WindowOrigin.X);
            }
            else
            {
                return Math.Round(
                    (x - _currentState.WindowOrigin.X)
                    * (_currentState.ViewportExtX / _currentState.WindowExtX)
                    + _currentState.ViewportOrigin.X,
                    0,
                    MidpointRounding.ToEven);
            }
        }

        public double ScaleHeight(double height)
        {
            if (_currentState.ViewportExtY == 0)
            {
                return height;
            }
            else
            {
                return height * (_currentState.ViewportExtY / _currentState.WindowExtY);
            }
        }

        public double ScaleWidth(double width)
        {
            if (_currentState.ViewportExtX == 0)
            {
                return width;
            }
            else
            {
                return width * (_currentState.ViewportExtX / _currentState.WindowExtX);
            }
        }

        private LogRegion GetDefaultClipRegion()
        {
            var region = new LogRegion
                             {
                                 Geometry = new RectangleGeometry
                                                {
                                                    Rect =
                                                        new Rect(new Point(int.MinValue, int.MinValue),
                                                                 new Point(int.MaxValue, int.MaxValue))
                                                }
                             };

            return region;
        }

        private Geometry GetClipGeometryAsChild(Rect objectBounds)
        {
            Geometry clipGeometry = null;

#if !NETFX_CORE
            if (_currentState.CurrentClipRegion != null && (_currentState.CurrentFont == null || _currentState.CurrentFont.Escapement == 0))
            {
                clipGeometry = _currentState.CurrentClipRegion.Geometry;

                if (clipGeometry.FillContains(new RectangleGeometry { Rect = objectBounds }))
                    // Clip fully contains the object, not needed
                    return null;

                clipGeometry = Geometry.Combine(
                                    new RectangleGeometry { Rect = objectBounds },
                                    clipGeometry,
                                    GeometryCombineMode.Intersect,
                                    null);

                if (clipGeometry.Bounds == Rect.Empty)
                {
                    clipGeometry = null;
                }
                else
                {
                    // Offset to zero
                    clipGeometry = Geometry.Combine(
                        Geometry.Empty,
                        clipGeometry,
                        GeometryCombineMode.Union,
                        new TranslateTransform
                            {
                                X = -objectBounds.X,
                                Y = -objectBounds.Y
                            }
                        );
                }
            }
#endif

            return clipGeometry;
        }

        private Geometry GetClipGeometry(Rect objectBounds)
        {
            Geometry clipGeometry = null;

#if !NETFX_CORE
            if (_currentState.CurrentClipRegion != null && (_currentState.CurrentFont == null || _currentState.CurrentFont.Escapement == 0))
            {
                clipGeometry = _currentState.CurrentClipRegion.Geometry;

                if (clipGeometry.FillContains(new RectangleGeometry { Rect = objectBounds }))
                    // Clip fully contains the object, not needed
                    return null;
            }
#endif

            return clipGeometry;
        }

        private void SaveImage(Image source)
        {

#if !NETFX_CORE
            string bitmapPath = ImageSavePath + _nextImageIndex.ToString(System.Globalization.CultureInfo.CurrentCulture) + ".bmp";
            var bitmap = (BitmapSource)source.Source;

            // Save original image
            using (var fileStream = new System.IO.FileStream(bitmapPath, System.IO.FileMode.Create))
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }

            // Reference saved image
            source.Source = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));

            _nextImageIndex++;
#endif
        }

        public int GetCharSet()
        {
            if (_currentState.CurrentFont == null)
            {
                return 1252;
            }
            else
            {
                return _currentState.CurrentFont.Charset;
            }
        }

        //private Geometry ScaleGeometry(double x, double y, Geometry data)
        //{
        //    var scaling = new TransformGroup();

        //    scaling.Children.Add(
        //        new TranslateTransform
        //            {
        //                X = -_currentState.WindowOrigin.X,
        //                Y = -_currentState.WindowOrigin.Y
        //            }
        //        );

        //    if (_currentState.ViewportExtY != 0)
        //    {
        //        scaling.Children.Add(
        //            new ScaleTransform()
        //                {
        //                    ScaleX = _currentState.ViewportExtX / _currentState.WindowExtX,
        //                    ScaleY = _currentState.ViewportExtY / _currentState.WindowExtY
        //                }
        //            );
        //    }

        //    scaling.Children.Add(
        //        new TranslateTransform
        //            {
        //                X = _currentState.ViewportOrigin.X,
        //                Y = _currentState.ViewportOrigin.Y
        //            }
        //        );

        //    return Geometry.Combine(Geometry.Empty, data, GeometryCombineMode.Union, scaling);
        //}

        //PathGeometry CreatePointGeometry(IEnumerable<Point> points, bool isFilled, bool isClosed)
        //{
        //    var geometry = new PathGeometry();

        //    geometry.Figures.Add(CreateFigures(points, isFilled, isClosed));

        //    return geometry;
        //}

        //PathGeometry CreatePointGeometry(IEnumerable<IEnumerable<Point>> points, bool isFilled, bool isClosed)
        //{
        //    var geometry = new PathGeometry();

        //    foreach (IEnumerable<Point> figurePoints in points)
        //    {
        //        geometry.Figures.Add(CreateFigures(figurePoints, isFilled, isClosed));
        //    }

        //    return geometry;
        //}

        //PathFigure CreateFigures(IEnumerable<Point> points, bool isFilled, bool isClosed)
        //{
        //    var segments = new PathSegmentCollection();
        //    var startPoint = new Point();
        //    var i = 0;

        //    foreach (var point in points)
        //    {
        //        if (i == 0)
        //        {
        //            startPoint = point;
        //        }
        //        else
        //        {
        //            segments.Add(new LineSegment
        //                    {
        //                        Point = point
        //                    }
        //                    );
        //        }

        //        i++;
        //    }

        //    return new PathFigure
        //        {
        //            StartPoint = startPoint,
        //            IsClosed = isClosed,
        //            IsFilled = isFilled,
        //            Segments = segments
        //        };
        //}
    }
}
