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
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return String.Format("{0:0.0} {1}", size, units[unit]);
        }
    }
}
