using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Utils
{
    class ExtensionHelper
    {
        private static Dictionary<string, string> extensionCache = new Dictionary<string, string>();
        public static string GetExtension(string mimeType, string fileName)
        {
            string result;
            if (!extensionCache.TryGetValue(mimeType, out result))
            {
                var baseKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("MIME").OpenSubKey("Database").OpenSubKey("Content Type").OpenSubKey(mimeType);
                if (baseKey == null) return System.IO.Path.GetExtension(fileName);
                result = (string)baseKey.GetValue("Extension");
                extensionCache[mimeType] = result;
            }
            return result;
        }
    }
}
