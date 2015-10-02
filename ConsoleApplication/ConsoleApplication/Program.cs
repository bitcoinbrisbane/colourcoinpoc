using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            const String BobAddress = "mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax";
            const String BobAssetAddress = "bX8Qc1nYCZT65cRjcbjg2wyaSjSWhY2gB5p";
            const String BobWIFKey = "cMdLBsUCQ92VSRmqfEL4TgJCisWpjVBd8GsP2mAmUZxQ9bh5E7CN";

            const String AliceAddress = "muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF";
            const String AliceAssetAddress = "bX5Gcpc75cdDxE2jcgXaLEuj5dEdBUdnpDu";
            const String AlicePrivateWIFKey = "cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH";

            BitcoinSecret alice = new BitcoinSecret(AlicePrivateWIFKey);
            BitcoinSecret bob = new BitcoinSecret(BobWIFKey);

            const String SILVER = "oLcV5F6R59zbChoTBQ962Har24tAnhbtHo";
            const String GOLD = "oWoYDF1Fxc7pSSoo97MMeH5NAmpqZhq5Ys";

            String result = Transfer(BobAddress, 1, AliceAddress, SILVER, BobWIFKey);
            Console.WriteLine(result);
            Console.ReadLine();
        }

        public static string Transfer(String fromAddress, Int64 amount, String toAddress, String assetId, String senderWifKey)
        {
            NBitcoin.Network network = NBitcoin.Network.TestNet;
            //NBitcoin.BitcoinSecret _key = NBitcoin.Network.TestNet.CreateBitcoinSecret(senderWifKey);
            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            var ccutoxs = txRepo.GetTransactions(fromAddress);

            //Find output which contains an incoming asset
            var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);

            //Colour coin utxo that was sent
            var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
                fromOutputIndex: Convert.ToUInt32(assetOutput.index),
                amount: Money.Satoshis(600), //default fee
                scriptPubKey: BitcoinAddress.Create(fromAddress).ScriptPubKey);
                //scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);

            //Arbitary coin
            var forfees = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
                fromOutputIndex: fundingUTXO.output_index,
                amount: Money.Satoshis(fundingUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            var alice = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));

            //FROM NIC
            var bobKey = new BitcoinSecret("cMdLBsUCQ92VSRmqfEL4TgJCisWpjVBd8GsP2mAmUZxQ9bh5E7CN");
            var aliceKey = new BitcoinSecret("cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH");
            var txBuilder = new TransactionBuilder();
            
            var tx = txBuilder
                .AddKeys(bobKey, aliceKey)
                .AddCoins(forfees, colored)
                .SendAsset(alice, new AssetMoney(assetIdx, Convert.ToUInt64(amount)))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001m))
                .BuildTransaction(true);

            var ok = txBuilder.Verify(tx);
            Submit(tx);
            return "ok";
        }

        private static ColoredCoin GetCoin(String assetId, UInt64 amount)
        {
            //Gold
            if (assetId == "")
            {

            }

            var goldCoin = new Coin(fromTxHash: new uint256("dc19133d57bf9013d898bd89198069340d8ca99d71f0d5f6c6e142d724a9ba92"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(600), //default fee
                scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);

            BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            return goldCoin.ToColoredCoin(assetIdx, amount);

        }

        public static void Submit(Transaction tx)
        {
            IList<String> nodes = new List<String>(3);
            nodes.Add("54.149.133.4:18333");
            //SOME TEST NET NODES
            //54.149.133.4:18333
            //52.69.206.155:18333
            //93.114.160.222:18333
            string url = "54.149.133.4:18333"; // "93.114.160.222:18333";
            using(var node = NBitcoin.Protocol.Node.Connect(NBitcoin.Network.TestNet, url))
            {
                node.VersionHandshake();
                //System.Threading.Thread.Sleep(1000);

                NBitcoin.Transaction[] transactions = new Transaction[1];
                transactions[0] = tx;

                node.SendMessage(new NBitcoin.Protocol.InvPayload(transactions));
                node.SendMessage(new NBitcoin.Protocol.TxPayload(tx));
            }
        }
    }
}
