using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BITM.BlazeSDK.Blaze;
using static BITM.BlazeSDK.Enums;

namespace BITM.BlazeSDK
{
    public struct IBlazeComponent
    {
        public string Component { get; set; }
        public string Command { get; set; }
        public dynamic Data { get; set; }

    }
}
