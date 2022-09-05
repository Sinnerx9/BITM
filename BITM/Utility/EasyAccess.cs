using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BITM.Utility
{
    public static class EasyAccess
    {
        public static MemoryStream toMemoryStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }
    }
}
