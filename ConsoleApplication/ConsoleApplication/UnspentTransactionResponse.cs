using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class UnspentTransactionResponse
    {
        public String transaction_hash { get; set; }

        public uint output_index { get; set; }

        public Int64 value { get; set; }

        public String asset_id { get; set; }

        public Decimal? asset_quantity { get; set; }

        public String[] addresses { get; set; }

        public String script_hex { get; set; }

        public Boolean spent { get; set; }
//03    "transaction_hash": "757624c8b29d47257c84962e7ebe538fb8e33fdc00abea311af3babd16db4742",
//04    "output_index": 2,
//05    "value": 600,
//06    "asset_id": null,
//07    "asset_quantity": null,
//08    "addresses": [
//09      "1HHYPRCFijLkwQFsEPLv6iNEkYiwB4qsgs"
//10    ],
//11    "script_hex": "76a914b2a2de394ce286509c8e4c30dcd157df001cdf7388ac",
//12    "spent": false
//13  },
    }
}
