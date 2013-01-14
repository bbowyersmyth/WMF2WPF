#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace WPFGDI
{
    public class LogPen : LogObject
	{
        public Color Colour { get; set; }
        public ushort Style { get; set; }
        public short Width { get; set; }
	}
}
