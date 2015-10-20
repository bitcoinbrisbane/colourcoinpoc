using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class CoinPrism
    {
        private String _baseUrl;

        public Boolean TestNet { get; set; }

        public CoinPrism(Boolean testNet = true)
        {
            TestNet = TestNet;

            if (testNet == true)
            {
                _baseUrl = "https://testnet.api.coinprism.com/";
            }
            else
            {
                _baseUrl = "https://api.coinprism.com/";
            }
        }

        /// <summary>
        /// Property inject user address
        /// </summary>
        public String UserAddress { get; set; }

        public List<UnspentTransactionResponse> GetUnspent(String fromAddress)
        {
            //api.coinprism.com/v1/addresses/address/unspents
            WebClient client = new WebClient();
            String response = client.DownloadString(String.Format("{0}/v1/addresses/{1}/unspents", _baseUrl, fromAddress));

            List<UnspentTransactionResponse> transactionResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UnspentTransactionResponse>>(response);

            return transactionResponse;
        }

        public IEnumerable<TransactionResponse> GetTransactions(String address)
        {
            WebClient client = new WebClient();
            String response = client.DownloadString(String.Format("{0}/v1/addresses/{1}/transactions?format=json", _baseUrl, address));

            IEnumerable<TransactionResponse> txs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TransactionResponse>>(response);
            return txs;
        }
    }
}
