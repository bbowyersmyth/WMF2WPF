﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace WPFGDI
{
    public static class Palettes
    {
        public static IList<Color> Halftone256
        {
            get
            {
                var colorList = new List<Color>();

                colorList.Add(Color.FromArgb(255, 0, 0, 0));
                colorList.Add(Color.FromArgb(255, 0, 0, 85));
                colorList.Add(Color.FromArgb(255, 0, 0, 170));
                colorList.Add(Color.FromArgb(255, 0, 0, 255));
                colorList.Add(Color.FromArgb(255, 0, 36, 0));
                colorList.Add(Color.FromArgb(255, 0, 36, 85));
                colorList.Add(Color.FromArgb(255, 0, 36, 170));
                colorList.Add(Color.FromArgb(255, 0, 36, 255));
                colorList.Add(Color.FromArgb(255, 0, 73, 0));
                colorList.Add(Color.FromArgb(255, 0, 73, 85));
                colorList.Add(Color.FromArgb(255, 0, 73, 170));
                colorList.Add(Color.FromArgb(255, 0, 73, 255));
                colorList.Add(Color.FromArgb(255, 0, 109, 0));
                colorList.Add(Color.FromArgb(255, 0, 109, 85));
                colorList.Add(Color.FromArgb(255, 0, 109, 170));
                colorList.Add(Color.FromArgb(255, 0, 109, 255));
                colorList.Add(Color.FromArgb(255, 0, 146, 0));
                colorList.Add(Color.FromArgb(255, 0, 146, 85));
                colorList.Add(Color.FromArgb(255, 0, 146, 170));
                colorList.Add(Color.FromArgb(255, 0, 146, 255));
                colorList.Add(Color.FromArgb(255, 0, 182, 0));
                colorList.Add(Color.FromArgb(255, 0, 182, 85));
                colorList.Add(Color.FromArgb(255, 0, 182, 170));
                colorList.Add(Color.FromArgb(255, 0, 182, 255));
                colorList.Add(Color.FromArgb(255, 0, 219, 0));
                colorList.Add(Color.FromArgb(255, 0, 219, 85));
                colorList.Add(Color.FromArgb(255, 0, 219, 170));
                colorList.Add(Color.FromArgb(255, 0, 219, 255));
                colorList.Add(Color.FromArgb(255, 0, 255, 0));
                colorList.Add(Color.FromArgb(255, 0, 255, 85));
                colorList.Add(Color.FromArgb(255, 0, 255, 170));
                colorList.Add(Color.FromArgb(255, 0, 255, 255));
                colorList.Add(Color.FromArgb(255, 36, 0, 0));
                colorList.Add(Color.FromArgb(255, 36, 0, 85));
                colorList.Add(Color.FromArgb(255, 36, 0, 170));
                colorList.Add(Color.FromArgb(255, 36, 0, 255));
                colorList.Add(Color.FromArgb(255, 36, 36, 0));
                colorList.Add(Color.FromArgb(255, 36, 36, 85));
                colorList.Add(Color.FromArgb(255, 36, 36, 170));
                colorList.Add(Color.FromArgb(255, 36, 36, 255));
                colorList.Add(Color.FromArgb(255, 36, 73, 0));
                colorList.Add(Color.FromArgb(255, 36, 73, 85));
                colorList.Add(Color.FromArgb(255, 36, 73, 170));
                colorList.Add(Color.FromArgb(255, 36, 73, 255));
                colorList.Add(Color.FromArgb(255, 36, 109, 0));
                colorList.Add(Color.FromArgb(255, 36, 109, 85));
                colorList.Add(Color.FromArgb(255, 36, 109, 170));
                colorList.Add(Color.FromArgb(255, 36, 109, 255));
                colorList.Add(Color.FromArgb(255, 36, 146, 0));
                colorList.Add(Color.FromArgb(255, 36, 146, 85));
                colorList.Add(Color.FromArgb(255, 36, 146, 170));
                colorList.Add(Color.FromArgb(255, 36, 146, 255));
                colorList.Add(Color.FromArgb(255, 36, 182, 0));
                colorList.Add(Color.FromArgb(255, 36, 182, 85));
                colorList.Add(Color.FromArgb(255, 36, 182, 170));
                colorList.Add(Color.FromArgb(255, 36, 182, 255));
                colorList.Add(Color.FromArgb(255, 36, 219, 0));
                colorList.Add(Color.FromArgb(255, 36, 219, 85));
                colorList.Add(Color.FromArgb(255, 36, 219, 170));
                colorList.Add(Color.FromArgb(255, 36, 219, 255));
                colorList.Add(Color.FromArgb(255, 36, 255, 0));
                colorList.Add(Color.FromArgb(255, 36, 255, 85));
                colorList.Add(Color.FromArgb(255, 36, 255, 170));
                colorList.Add(Color.FromArgb(255, 36, 255, 255));
                colorList.Add(Color.FromArgb(255, 73, 0, 0));
                colorList.Add(Color.FromArgb(255, 73, 0, 85));
                colorList.Add(Color.FromArgb(255, 73, 0, 170));
                colorList.Add(Color.FromArgb(255, 73, 0, 255));
                colorList.Add(Color.FromArgb(255, 73, 36, 0));
                colorList.Add(Color.FromArgb(255, 73, 36, 85));
                colorList.Add(Color.FromArgb(255, 73, 36, 170));
                colorList.Add(Color.FromArgb(255, 73, 36, 255));
                colorList.Add(Color.FromArgb(255, 73, 73, 0));
                colorList.Add(Color.FromArgb(255, 73, 73, 85));
                colorList.Add(Color.FromArgb(255, 73, 73, 170));
                colorList.Add(Color.FromArgb(255, 73, 73, 255));
                colorList.Add(Color.FromArgb(255, 73, 109, 0));
                colorList.Add(Color.FromArgb(255, 73, 109, 85));
                colorList.Add(Color.FromArgb(255, 73, 109, 170));
                colorList.Add(Color.FromArgb(255, 73, 109, 255));
                colorList.Add(Color.FromArgb(255, 73, 146, 0));
                colorList.Add(Color.FromArgb(255, 73, 146, 85));
                colorList.Add(Color.FromArgb(255, 73, 146, 170));
                colorList.Add(Color.FromArgb(255, 73, 146, 255));
                colorList.Add(Color.FromArgb(255, 73, 182, 0));
                colorList.Add(Color.FromArgb(255, 73, 182, 85));
                colorList.Add(Color.FromArgb(255, 73, 182, 170));
                colorList.Add(Color.FromArgb(255, 73, 182, 255));
                colorList.Add(Color.FromArgb(255, 73, 219, 0));
                colorList.Add(Color.FromArgb(255, 73, 219, 85));
                colorList.Add(Color.FromArgb(255, 73, 219, 170));
                colorList.Add(Color.FromArgb(255, 73, 219, 255));
                colorList.Add(Color.FromArgb(255, 73, 255, 0));
                colorList.Add(Color.FromArgb(255, 73, 255, 85));
                colorList.Add(Color.FromArgb(255, 73, 255, 170));
                colorList.Add(Color.FromArgb(255, 73, 255, 255));
                colorList.Add(Color.FromArgb(255, 109, 0, 0));
                colorList.Add(Color.FromArgb(255, 109, 0, 85));
                colorList.Add(Color.FromArgb(255, 109, 0, 170));
                colorList.Add(Color.FromArgb(255, 109, 0, 255));
                colorList.Add(Color.FromArgb(255, 109, 36, 0));
                colorList.Add(Color.FromArgb(255, 109, 36, 85));
                colorList.Add(Color.FromArgb(255, 109, 36, 170));
                colorList.Add(Color.FromArgb(255, 109, 36, 255));
                colorList.Add(Color.FromArgb(255, 109, 73, 0));
                colorList.Add(Color.FromArgb(255, 109, 73, 85));
                colorList.Add(Color.FromArgb(255, 109, 73, 170));
                colorList.Add(Color.FromArgb(255, 109, 73, 255));
                colorList.Add(Color.FromArgb(255, 109, 109, 0));
                colorList.Add(Color.FromArgb(255, 109, 109, 85));
                colorList.Add(Color.FromArgb(255, 109, 109, 170));
                colorList.Add(Color.FromArgb(255, 109, 109, 255));
                colorList.Add(Color.FromArgb(255, 109, 146, 0));
                colorList.Add(Color.FromArgb(255, 109, 146, 85));
                colorList.Add(Color.FromArgb(255, 109, 146, 170));
                colorList.Add(Color.FromArgb(255, 109, 146, 255));
                colorList.Add(Color.FromArgb(255, 109, 182, 0));
                colorList.Add(Color.FromArgb(255, 109, 182, 85));
                colorList.Add(Color.FromArgb(255, 109, 182, 170));
                colorList.Add(Color.FromArgb(255, 109, 182, 255));
                colorList.Add(Color.FromArgb(255, 109, 219, 0));
                colorList.Add(Color.FromArgb(255, 109, 219, 85));
                colorList.Add(Color.FromArgb(255, 109, 219, 170));
                colorList.Add(Color.FromArgb(255, 109, 219, 255));
                colorList.Add(Color.FromArgb(255, 109, 255, 0));
                colorList.Add(Color.FromArgb(255, 109, 255, 85));
                colorList.Add(Color.FromArgb(255, 109, 255, 170));
                colorList.Add(Color.FromArgb(255, 109, 255, 255));
                colorList.Add(Color.FromArgb(255, 146, 0, 0));
                colorList.Add(Color.FromArgb(255, 146, 0, 85));
                colorList.Add(Color.FromArgb(255, 146, 0, 170));
                colorList.Add(Color.FromArgb(255, 146, 0, 255));
                colorList.Add(Color.FromArgb(255, 146, 36, 0));
                colorList.Add(Color.FromArgb(255, 146, 36, 85));
                colorList.Add(Color.FromArgb(255, 146, 36, 170));
                colorList.Add(Color.FromArgb(255, 146, 36, 255));
                colorList.Add(Color.FromArgb(255, 146, 73, 0));
                colorList.Add(Color.FromArgb(255, 146, 73, 85));
                colorList.Add(Color.FromArgb(255, 146, 73, 170));
                colorList.Add(Color.FromArgb(255, 146, 73, 255));
                colorList.Add(Color.FromArgb(255, 146, 109, 0));
                colorList.Add(Color.FromArgb(255, 146, 109, 85));
                colorList.Add(Color.FromArgb(255, 146, 109, 170));
                colorList.Add(Color.FromArgb(255, 146, 109, 255));
                colorList.Add(Color.FromArgb(255, 146, 146, 0));
                colorList.Add(Color.FromArgb(255, 146, 146, 85));
                colorList.Add(Color.FromArgb(255, 146, 146, 170));
                colorList.Add(Color.FromArgb(255, 146, 146, 255));
                colorList.Add(Color.FromArgb(255, 146, 182, 0));
                colorList.Add(Color.FromArgb(255, 146, 182, 85));
                colorList.Add(Color.FromArgb(255, 146, 182, 170));
                colorList.Add(Color.FromArgb(255, 146, 182, 255));
                colorList.Add(Color.FromArgb(255, 146, 219, 0));
                colorList.Add(Color.FromArgb(255, 146, 219, 85));
                colorList.Add(Color.FromArgb(255, 146, 219, 170));
                colorList.Add(Color.FromArgb(255, 146, 219, 255));
                colorList.Add(Color.FromArgb(255, 146, 255, 0));
                colorList.Add(Color.FromArgb(255, 146, 255, 85));
                colorList.Add(Color.FromArgb(255, 146, 255, 170));
                colorList.Add(Color.FromArgb(255, 146, 255, 255));
                colorList.Add(Color.FromArgb(255, 182, 0, 0));
                colorList.Add(Color.FromArgb(255, 182, 0, 85));
                colorList.Add(Color.FromArgb(255, 182, 0, 170));
                colorList.Add(Color.FromArgb(255, 182, 0, 255));
                colorList.Add(Color.FromArgb(255, 182, 36, 0));
                colorList.Add(Color.FromArgb(255, 182, 36, 85));
                colorList.Add(Color.FromArgb(255, 182, 36, 170));
                colorList.Add(Color.FromArgb(255, 182, 36, 255));
                colorList.Add(Color.FromArgb(255, 182, 73, 0));
                colorList.Add(Color.FromArgb(255, 182, 73, 85));
                colorList.Add(Color.FromArgb(255, 182, 73, 170));
                colorList.Add(Color.FromArgb(255, 182, 73, 255));
                colorList.Add(Color.FromArgb(255, 182, 109, 0));
                colorList.Add(Color.FromArgb(255, 182, 109, 85));
                colorList.Add(Color.FromArgb(255, 182, 109, 170));
                colorList.Add(Color.FromArgb(255, 182, 109, 255));
                colorList.Add(Color.FromArgb(255, 182, 146, 0));
                colorList.Add(Color.FromArgb(255, 182, 146, 85));
                colorList.Add(Color.FromArgb(255, 182, 146, 170));
                colorList.Add(Color.FromArgb(255, 182, 146, 255));
                colorList.Add(Color.FromArgb(255, 182, 182, 0));
                colorList.Add(Color.FromArgb(255, 182, 182, 85));
                colorList.Add(Color.FromArgb(255, 182, 182, 170));
                colorList.Add(Color.FromArgb(255, 182, 182, 255));
                colorList.Add(Color.FromArgb(255, 182, 219, 0));
                colorList.Add(Color.FromArgb(255, 182, 219, 85));
                colorList.Add(Color.FromArgb(255, 182, 219, 170));
                colorList.Add(Color.FromArgb(255, 182, 219, 255));
                colorList.Add(Color.FromArgb(255, 182, 255, 0));
                colorList.Add(Color.FromArgb(255, 182, 255, 85));
                colorList.Add(Color.FromArgb(255, 182, 255, 170));
                colorList.Add(Color.FromArgb(255, 182, 255, 255));
                colorList.Add(Color.FromArgb(255, 219, 0, 0));
                colorList.Add(Color.FromArgb(255, 219, 0, 85));
                colorList.Add(Color.FromArgb(255, 219, 0, 170));
                colorList.Add(Color.FromArgb(255, 219, 0, 255));
                colorList.Add(Color.FromArgb(255, 219, 36, 0));
                colorList.Add(Color.FromArgb(255, 219, 36, 85));
                colorList.Add(Color.FromArgb(255, 219, 36, 170));
                colorList.Add(Color.FromArgb(255, 219, 36, 255));
                colorList.Add(Color.FromArgb(255, 219, 73, 0));
                colorList.Add(Color.FromArgb(255, 219, 73, 85));
                colorList.Add(Color.FromArgb(255, 219, 73, 170));
                colorList.Add(Color.FromArgb(255, 219, 73, 255));
                colorList.Add(Color.FromArgb(255, 219, 109, 0));
                colorList.Add(Color.FromArgb(255, 219, 109, 85));
                colorList.Add(Color.FromArgb(255, 219, 109, 170));
                colorList.Add(Color.FromArgb(255, 219, 109, 255));
                colorList.Add(Color.FromArgb(255, 219, 146, 0));
                colorList.Add(Color.FromArgb(255, 219, 146, 85));
                colorList.Add(Color.FromArgb(255, 219, 146, 170));
                colorList.Add(Color.FromArgb(255, 219, 146, 255));
                colorList.Add(Color.FromArgb(255, 219, 182, 0));
                colorList.Add(Color.FromArgb(255, 219, 182, 85));
                colorList.Add(Color.FromArgb(255, 219, 182, 170));
                colorList.Add(Color.FromArgb(255, 219, 182, 255));
                colorList.Add(Color.FromArgb(255, 219, 219, 0));
                colorList.Add(Color.FromArgb(255, 219, 219, 85));
                colorList.Add(Color.FromArgb(255, 219, 219, 170));
                colorList.Add(Color.FromArgb(255, 219, 219, 255));
                colorList.Add(Color.FromArgb(255, 219, 255, 0));
                colorList.Add(Color.FromArgb(255, 219, 255, 85));
                colorList.Add(Color.FromArgb(255, 219, 255, 170));
                colorList.Add(Color.FromArgb(255, 219, 255, 255));
                colorList.Add(Color.FromArgb(255, 255, 0, 0));
                colorList.Add(Color.FromArgb(255, 255, 0, 85));
                colorList.Add(Color.FromArgb(255, 255, 0, 170));
                colorList.Add(Color.FromArgb(255, 255, 0, 255));
                colorList.Add(Color.FromArgb(255, 255, 36, 0));
                colorList.Add(Color.FromArgb(255, 255, 36, 85));
                colorList.Add(Color.FromArgb(255, 255, 36, 170));
                colorList.Add(Color.FromArgb(255, 255, 36, 255));
                colorList.Add(Color.FromArgb(255, 255, 73, 0));
                colorList.Add(Color.FromArgb(255, 255, 73, 85));
                colorList.Add(Color.FromArgb(255, 255, 73, 170));
                colorList.Add(Color.FromArgb(255, 255, 73, 255));
                colorList.Add(Color.FromArgb(255, 255, 109, 0));
                colorList.Add(Color.FromArgb(255, 255, 109, 85));
                colorList.Add(Color.FromArgb(255, 255, 109, 170));
                colorList.Add(Color.FromArgb(255, 255, 109, 255));
                colorList.Add(Color.FromArgb(255, 255, 146, 0));
                colorList.Add(Color.FromArgb(255, 255, 146, 85));
                colorList.Add(Color.FromArgb(255, 255, 146, 170));
                colorList.Add(Color.FromArgb(255, 255, 146, 255));
                colorList.Add(Color.FromArgb(255, 255, 182, 0));
                colorList.Add(Color.FromArgb(255, 255, 182, 85));
                colorList.Add(Color.FromArgb(255, 255, 182, 170));
                colorList.Add(Color.FromArgb(255, 255, 182, 255));
                colorList.Add(Color.FromArgb(255, 255, 219, 0));
                colorList.Add(Color.FromArgb(255, 255, 219, 85));
                colorList.Add(Color.FromArgb(255, 255, 219, 170));
                colorList.Add(Color.FromArgb(255, 255, 219, 255));
                colorList.Add(Color.FromArgb(255, 255, 255, 0));
                colorList.Add(Color.FromArgb(255, 255, 255, 85));
                colorList.Add(Color.FromArgb(255, 255, 255, 170));
                colorList.Add(Color.FromArgb(255, 255, 255, 255));

                return colorList;
            }
        }
    }
}
