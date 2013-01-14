#if NETFX_CORE
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace WPFGDI
{
	public class LogBrush : LogObject
	{
        public BrushStyle Style { get; set; }
        public Color Colour { get; set; }
        public HatchStyle Hatch { get; set; }
        public BitmapSource Image { get; set; }
	}
}
