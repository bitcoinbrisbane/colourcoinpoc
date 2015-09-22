using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class Output
    {
        public Int64 index { get; set; }

        public String transaction_hash { get; set; }

        public String output_hash { get; set; }

        public Int64 value { get; set; }

        public String asset_id { get; set; }

        public Int64? asset_quantity { get; set; }
    }
}
