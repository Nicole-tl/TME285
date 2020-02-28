using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AudioLibrary.Visualization
{
    public class HighLight
    {
        public double Start { get; set; }
        public double End { get; set; }
        public Color Color { get; set; }
        public int Opacity { get; set; }
    }
}
