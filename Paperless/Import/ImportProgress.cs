using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paperless.Import
{
    public class ImportProgress
    {
        public string Title { get; set; }
        public int MaxValue { get; set; }
        public int Progress { get; set; }
        public string Text { get; set; }
    }
}
