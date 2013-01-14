using System.Windows;
#if NETFX_CORE
using Windows.UI;
using Windows.Foundation;
#else
using System.Windows.Media;
#endif

namespace WPFGDI
{
    internal class State
    {
        public Color BackgroundColour { get; set; }
        public MixMode BackgroundMode { get; set; }
        public BinaryRasterOperation DrawMode { get; set; }
        public MapMode MapMode { get; set; }  // TODO
        public PolyFillMode PolyFillMode { get; set; }
        public LogBrush CurrentBrush { get; set; }
        public LogRegion CurrentClipRegion { get; set; }
        public LogPalette CurrentPalette { get; set; }
        public LogPen CurrentPen { get; set; }
        public LogFont CurrentFont { get; set; }
        public Point CurrentPosition { get; set; }
        //public BitmapPalette RealizedPalette { get; set; }
        public StretchMode StretchBltMode { get; set; }
        public ushort TextAlign { get; set; }
        public Color TextColor { get; set; }
        public double ViewportExtX { get; set; }
        public double ViewportExtY { get; set; }
        public Point ViewportOrigin { get; set; }
        public double WindowExtX { get; set; }
        public double WindowExtY { get; set; }
        public Point WindowOrigin { get; set; }

        public State()
        {
            TextColor = Colors.Black;
        }

        public State Clone()
        {
            var newState = new State
                               {
                                   BackgroundMode = this.BackgroundMode,
                                   CurrentBrush = this.CurrentBrush,
                                   CurrentClipRegion = this.CurrentClipRegion,
                                   CurrentPalette = this.CurrentPalette,
                                   CurrentPen = this.CurrentPen,
                                   CurrentPosition = this.CurrentPosition,
                                   DrawMode = this.DrawMode,
                                   PolyFillMode = this.PolyFillMode,
                                   MapMode = this.MapMode,
                                   StretchBltMode = this.StretchBltMode,
                                   TextAlign = this.TextAlign,
                                   TextColor = this.TextColor,
                                   ViewportExtX = this.ViewportExtX,
                                   ViewportExtY = this.ViewportExtY,
                                   ViewportOrigin = this.ViewportOrigin,
                                   WindowExtX = this.WindowExtX,
                                   WindowExtY = this.WindowExtY,
                                   WindowOrigin = this.WindowOrigin
                                   //RealizedPalette = this.RealizedPalette;
                               };


            return newState;
        }
    }
}
