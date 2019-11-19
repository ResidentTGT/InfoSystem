using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pim.Options
{
    public class ExportTemplate
    {
        public int CategoryId { get; set; }
        public int[] Model { get; set; }
        public int[] ColorModel { get; set; }
        public int[] RangeSizeModel { get; set; }
    }
}