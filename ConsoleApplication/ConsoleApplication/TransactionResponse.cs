using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class TransactionResponse
    {
        public String hash { get; set; }

        public Input[] inputs { get; set; }
    }
}
