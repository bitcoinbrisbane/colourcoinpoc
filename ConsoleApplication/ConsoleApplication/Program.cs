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

            Transfer(BobAddress, 5, AliceAddress, SILVER, BobWIFKey);
        }

        public static string Transfer(String fromAddress, Int64 amount, String toAddress, String assetId, String senderWifKey)
        {


            NBitcoin.Network network = NBitcoin.Network.TestNet;
            NBitcoin.BitcoinSecret _key = NBitcoin.Network.TestNet.CreateBitcoinSecret(senderWifKey);


            //Get bitcoin balance, 0.0006 is required
            Decimal fundingBalance = 0;

            const Decimal BITCOIN_TRANSACTION_FEE = 0.00006M;

            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);
            var ccutoxs = txRepo.GetTransactions(fromAddress);
            //var utxos = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).ToList();

            var silverOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);


            //Find the coin bob sent
            //Coin from Bob
            var coin = new Coin(fromTxHash: new uint256("dc19133d57bf9013d898bd89198069340d8ca99d71f0d5f6c6e142d724a9ba92"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(600), //20000
                scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);

            //Coin from Alice
            var forfees = new Coin(fromTxHash: new uint256("b4326462d6d3b522d7e2c06d9c904313f546395eb62661b06b57195691f5fe5f"),
                fromOutputIndex: 1,
                amount: Money.Coins(1m), //9957600
                scriptPubKey: BitcoinAddress.Create("muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF").ScriptPubKey);


            //var coin = new Coin(fromTxHash: new uint256(utxos[0].transaction_hash),
            //    fromOutputIndex: utxos[0].output_index,
            //    amount: Money.Satoshis(60000), //20000
            //    scriptPubKey: new Script(Encoders.Hex.DecodeData(utxos[0].script_hex)));

            //var forfees = new Coin(fromTxHash: new uint256(utxos[1].transaction_hash),
            //    fromOutputIndex: utxos[1].output_index,
            //    amount: Money.Satoshis(60000), //20000
            //    scriptPubKey: new Script(Encoders.Hex.DecodeData(utxos[1].script_hex)));


            BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            var alice = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            ulong u = Convert.ToUInt64(amount);
            ColoredCoin colored = coin.ToColoredCoin(assetIdx, u);

            //var satoshi = Key.Parse(, Network.TestNet);
            string aliceWIF = "cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH";
            var satoshi = Key.Parse(aliceWIF, Network.TestNet);
            //;


            //FROM NIC
            var bobKey = new BitcoinSecret("cMdLBsUCQ92VSRmqfEL4TgJCisWpjVBd8GsP2mAmUZxQ9bh5E7CN");
            var aliceKey = new BitcoinSecret("cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH");
            var txBuilder = new TransactionBuilder();
            var tx = txBuilder
                .AddKeys(bobKey,aliceKey)
                .AddCoins(forfees, colored)
                .SendAsset(alice, new AssetMoney(assetIdx, u))
                //.SendAsset(satoshi.PubKey, new NBitcoin.OpenAsset.AssetMoney(assetIdx, u))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001m))
                .BuildTransaction(true);
            var ok = txBuilder.Verify(tx);
            Submit(tx);
            return "ok";
        }

        public static void Submit(Transaction tx)
        {
            //SOME TEST NET NODES
            //54.149.133.4:18333
            //52.69.206.155:18333
            //93.114.160.222:18333
            string url = "93.114.160.222:18333";
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
