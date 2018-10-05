using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Utils
{
    public static class Utils
    {
        public static string ReadableFileSize(this long size, int unit = 0)
        {
            var sizeF = (float)size;
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            while (sizeF >= 1024)
            {
                sizeF /= 1024;
                ++unit;
            }

            return String.Format("{0:0.0} {1}", sizeF, units[unit]);
        }
    }
}
