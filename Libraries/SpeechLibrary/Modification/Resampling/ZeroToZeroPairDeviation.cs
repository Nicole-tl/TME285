using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechLibrary.Modification.Resampling
{
    public class ZeroToZeroPairDeviation
    {
        public int StartIndex1 { get; set; }
        public int EndIndex1 { get; set; }
        public double StartTime1 { get; set; }
        public double EndTime1 { get; set; }
        public double Deviation { get; set;}
        public double RelativeDeviation { get; set; }
    }
}
