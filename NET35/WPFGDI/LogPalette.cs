using System.Collections.Generic;
#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace WPFGDI
{
    public class LogPalette : LogObject
    {
        public List<PaletteEntryFlag> Values { get; set; }
        public List<Color> Entries { get; set; }

        public LogPalette()
        {
            this.Values = new List<PaletteEntryFlag>();
            this.Entries = new List<Color>();
        }
    }
}
