using System.Collections.Generic;
using System.Windows;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace WPFGDI
{
    public class LogRegion : LogObject
    {
        private Geometry _regionGeometry;

        public List<List<Point>> Points { get; set; }

        public LogRegion()
        {
        }

        public LogRegion(Geometry geometry)
        {
            Geometry = geometry;
        }

        public LogRegion(List<List<Point>> points)
        {
            Points = points;
        }

        public Geometry Geometry
        {
            get
            {
                if (_regionGeometry == null & Points != null)
                {
#if NETFX_CORE
                    var geometry = new PathGeometry();
                    var figure = new PathFigure();

                    geometry.Figures.Add(figure);

                    foreach (var scanPoints in Points)
                    {
                        var i = 0;

                        foreach (var item in scanPoints)
                        {
                            if (i == 0)
                            {
                                // Move to
                                figure.IsClosed = true;
                                figure.StartPoint = item;
                            }
                            else
                            {
                                figure.Segments.Add(
                                    new LineSegment()
                                    {
                                        Point = item
                                    }
                                );
                            }

                            i++;
                        }
                    }
#else
                    // Create geometry from points
                    var geometry = new StreamGeometry();

                    using (var ctx = geometry.Open())
                    {
                        foreach (var scanPoints in Points)
                        {
                            var i = 0;

                            foreach (var item in scanPoints)
                            {
                                if (i == 0)
                                {
                                    // Move to
                                    ctx.BeginFigure(item, false, true);
                                }
                                else
                                {
                                    ctx.LineTo(item, false, false);
                                }

                                i++;
                            }
                        }
                    }
#endif

                    _regionGeometry = geometry;
                }

                return _regionGeometry;
            }

            set
            {
                this._regionGeometry = value;
            }
        }

        public LogRegion Clone()
        {
#if NETFX_CORE
            // TODO
            var copy = new LogRegion { Points = Points };
#else
            var copy = new LogRegion { Geometry = Geometry.Clone(), Points = Points };
#endif

            return copy;
        }
    }
}
