﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class TransactionResponse
    {
        public String hash { get; set; }

        public Int64 block_height { get; set; }

        public DateTime block_time { get; set; }

        public Int64 confirmations { get; set; }

        public IEnumerable<Input> inputs { get; set; }

        public IEnumerable<Output> outputs { get; set; }
    }
}
