using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AgentLibrary.Visualization
{
    [DataContract]
    public class MemoryItemVisualizationData
    {
        [DataMember]
        public double RelativeX { get; set; }
        [DataMember]
        public double RelativeY { get; set; }
        [DataMember]
        public double RelativeWidth { get; set; }
        [DataMember]
        public double RelativeHeight { get; set; }
        [DataMember]
        public Boolean HighLightOn { get; set; }
    }
}
