using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        const String BobWIFKey = "cMdLBsUCQ92VSRmqfEL4TgJCisWpjVBd8GsP2mAmUZxQ9bh5E7CN";
        const String AliceWIFKey = "cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH";

        static void Main(string[] args)
        {
            const String BobAddress = "mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax";
            const String BobAssetAddress = "bX8Qc1nYCZT65cRjcbjg2wyaSjSWhY2gB5p";


            const String AliceAddress = "muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF";
            const String AliceAssetAddress = "bX5Gcpc75cdDxE2jcgXaLEuj5dEdBUdnpDu";
            

            BitcoinSecret alice = new BitcoinSecret(AliceWIFKey);
            BitcoinSecret bob = new BitcoinSecret(BobWIFKey);

            const String SILVER = "oLcV5F6R59zbChoTBQ962Har24tAnhbtHo";
            const String GOLD = "oWoYDF1Fxc7pSSoo97MMeH5NAmpqZhq5Ys";
            const String USD = "oSANrbK92PePSWkmjP7FtVqLtHWPwFTKWc";
            const String AUD = "oM4tzMCMyxQ5zgtb3QtPkFaoBtQKBG2WdP";

            //String result = Transfer(BobAddress, 3, AliceAddress, GOLD, BobWIFKey);
            String result = Transfer(BobAddress, 10000, AliceAddress, AUD);

            Console.WriteLine(result);
            Console.ReadLine();
               
        }


        public static string Transfer(String fromAddress, Int64 amount, String toAddress, String assetId)
        {
            NBitcoin.Network network = NBitcoin.Network.TestNet;

            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress, network);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress, network);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            if (fundingUTXO == null)
            {
                throw new ArgumentNullException("Need more btc");
            }

            //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            var ccutoxs = txRepo.GetTransactions(fromAddress);

            //TODO:  CHANGE FOR DIVISIBILITY
            //Find output which contains an incoming asset
            var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId && o.asset_quantity >= amount);

            if (assetOutput == null)
            {
                throw new ArgumentNullException("Not enough assets");
            }

            //Colour coin utxo that was sent
            var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
                fromOutputIndex: Convert.ToUInt32(assetOutput.index),
                amount: Money.Satoshis(600), //default fee
                scriptPubKey: bitcoinFromAddress.ScriptPubKey);

            //Arbitary coin
            var forfees = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
                fromOutputIndex: fundingUTXO.output_index,
                amount: Money.Satoshis(fundingUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            var alice = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));

            //FROM NIC
            var bobKey = new BitcoinSecret(BobWIFKey);
            var aliceKey = new BitcoinSecret(AliceWIFKey);
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

            return tx.ToHex();
        }



        public static String FromNewAddressToAlice()
        {
            const String AliceAddress = "muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF";

            String senderKey = "93KgTTe7YrwEFtfcHYL9Bsm1p4PcQEfxaqEw3CBcUCbx17Xc4Nk";
            String senderAddress = "mtZk7YUMEK3rGWa91VfvYXAJaK8iw6DMvV";

            NBitcoin.Network network = NBitcoin.Network.TestNet;

            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(AliceAddress, network);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(senderAddress, network);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.Get(bitcoinFromAddress.ToString()).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            //var ccutoxs = txRepo.GetTransactions(fromAddress);

            //Find output which contains an incoming asset
            //var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);

            var coin_nic = new Coin(fromTxHash: new uint256("5a2359ec87780561306b2d6fbe4151704ef0c97ab09ab51548502d1431a96331"),
                fromOutputIndex: 1,
                amount: Money.Satoshis(100000000), //default fee
                scriptPubKey: BitcoinAddress.Create(senderAddress).ScriptPubKey);



            ////Colour coin utxo that was sent
            //var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
            //    fromOutputIndex: Convert.ToUInt32(assetOutput.index),
            //    amount: Money.Satoshis(600), //default fee
            //    scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);

            ////Arbitary coin
            //var forfees = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
            //    fromOutputIndex: fundingUTXO.output_index,
            //    amount: Money.Satoshis(fundingUTXO.value), //20000
            //    scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            //BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            //var alice = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            //ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));

            //FROM NIC
            var senderSecret = new BitcoinSecret(senderKey);
            //var aliceKey = new BitcoinSecret("cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH");
            var txBuilder = new TransactionBuilder();

            var tx = txBuilder
                .AddKeys(senderSecret)
                .AddCoins(coin_nic)
                .Send(bitcoinToAddress, new Money(1000))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001m))
                .BuildTransaction(true);

            var ok = txBuilder.Verify(tx);
            Submit(tx);
            return "ok";
        }

        /// <summary>
        /// Code as per nic
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="amount"></param>
        /// <param name="toAddress"></param>
        /// <param name="assetId"></param>
        /// <param name="senderWifKey"></param>
        /// <returns></returns>
        public static string TransferAsFromNic(String fromAddress, Int64 amount, String toAddress, String assetId, String senderWifKey)
        {
            NBitcoin.Network network = NBitcoin.Network.TestNet;
            NBitcoin.BitcoinSecret _key = NBitcoin.Network.TestNet.CreateBitcoinSecret(senderWifKey);
            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            var ccutoxs = txRepo.GetTransactions(fromAddress);

            //Find output which contains an incoming asset
            var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);

            var coin_nic = new Coin(fromTxHash: new uint256("dc19133d57bf9013d898bd89198069340d8ca99d71f0d5f6c6e142d724a9ba92"),
                fromOutputIndex: 0,
                amount: Money.Satoshis(600), //default fee
                scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);


            if (1 == 2)
            {
                //Find the coin bob sent
                //Coin from Bob

                //Coin from Alice
                var forfees_nic = new Coin(fromTxHash: new uint256("b4326462d6d3b522d7e2c06d9c904313f546395eb62661b06b57195691f5fe5f"),
                    fromOutputIndex: 1,
                    amount: Money.Coins(1m), //9957600
                    scriptPubKey: BitcoinAddress.Create("muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF").ScriptPubKey);
            }

            //Colour coin utxo that was sent
            var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
                fromOutputIndex: Convert.ToUInt32(assetOutput.index),
                amount: Money.Satoshis(600), //default fee
                scriptPubKey: BitcoinAddress.Create("mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax").ScriptPubKey);

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

        //public static string Transfer4(String fromAddress, Int64 amount, String toAddress, String assetId, String senderWifKey)
        //{
        //    NBitcoin.Network network = NBitcoin.Network.TestNet;
        //    //NBitcoin.BitcoinSecret _key = NBitcoin.Network.TestNet.CreateBitcoinSecret(senderWifKey);
        //    NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress);
        //    NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress);

        //    //UTXOS
        //    NBitcoin.BlockrTransactionRepository blkrRepo = new BlockrTransactionRepository(network);
        //    var x = blkrRepo.GetUnspentAsync(fromAddress).Result.ToList();
        //    CoinPrism txRepo = new CoinPrism(true);

        //    //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
        //    var fundingUTXO = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();
        //    Debug.Assert(fundingUTXO.spent == false);

        //    //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
        //    //55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e

        //    var ccutoxs = txRepo.GetTransactions(fromAddress);

        //   // var xxx = ccutoxs.Where(c => c.hash == "55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e").ToList();

        //    //Find output which contains an incoming asset
        //    var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);


        //    //Colour coin utxo that was sent
        //    var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
        //        fromOutputIndex: Convert.ToUInt32(assetOutput.index),
        //        amount: Money.Satoshis(600), //default fee
        //        scriptPubKey: BitcoinAddress.Create(fromAddress).ScriptPubKey);

        //    //var coin = new Coin(fromTxHash: new uint256(xxx[0].hash),
        //    //    fromOutputIndex: Convert.ToUInt32(1),
        //    //    amount: Money.Satoshis(600), //default fee
        //    //    scriptPubKey: BitcoinAddress.Create(fromAddress).ScriptPubKey);

            

        //    //Arbitary coin
        //    var forFees = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
        //        fromOutputIndex: fundingUTXO.output_index,
        //        amount: Money.Satoshis(fundingUTXO.value), //20000
        //        scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

        //    BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
        //    var alice = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

        //    ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));

        //    //FROM NIC
        //    var bobKey = new BitcoinSecret(BobWIFKey);

        //    //var aliceKey = new BitcoinSecret("cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH");
        //    var txBuilder = new TransactionBuilder();

        //    var tx = txBuilder
        //        .AddKeys(bobKey)
        //        .AddCoins(forFees)
        //        .Send(bitcoinToAddress, "0.0005")
        //        .SetChange(bitcoinFromAddress)
        //        .SendFees(Money.Coins(0.001m))
        //        .BuildTransaction(true);

        //    var ok = txBuilder.Verify(tx);
        //    Submit(tx);

        //    var hex = tx.ToHex();
        //    return hex;
        //}

        public static string SimpleTransfer(String fromAddress, Int64 amount, String toAddress, String assetId, String senderWifKey)
        {
            NBitcoin.Network network = NBitcoin.Network.TestNet;
            //NBitcoin.BitcoinSecret _key = NBitcoin.Network.TestNet.CreateBitcoinSecret(senderWifKey);
            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress, network);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress, network);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.Get(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            //55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            
            //var ccutoxs = txRepo.GetTransactions(fromAddress);

            //var xxx = ccutoxs.Where(c => c.hash == "55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e").ToList();

            //Find output which contains an incoming asset
            //var assetOutput = ccutoxs.FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)).outputs.FirstOrDefault(o => o.asset_id == assetId);


            ////Colour coin utxo that was sent
            //var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
            //    fromOutputIndex: Convert.ToUInt32(assetOutput.index),
            //    amount: Money.Satoshis(600), //default fee
            //    scriptPubKey: BitcoinAddress.Create(fromAddress).ScriptPubKey);

            //var coin = new Coin(fromTxHash: new uint256(xxx[0].hash),
            //    fromOutputIndex: Convert.ToUInt32(1),
            //    amount: Money.Satoshis(600), //default fee
            //    scriptPubKey: BitcoinAddress.Create(fromAddress).ScriptPubKey);

            //Arbitary coin
            var coinsToSend = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
                fromOutputIndex: fundingUTXO.output_index,
                amount: Money.Satoshis(fundingUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            //BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            //var toAddress = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            //ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));

            //FROM NIC
            //var bobKey = new BitcoinSecret(BobWIFKey);
            var aliceKey = new BitcoinSecret(AliceWIFKey);
            var txBuilder = new TransactionBuilder();

            var tx = txBuilder
                .AddKeys(aliceKey)
                .AddCoins(coinsToSend)
                .Send(bitcoinToAddress, Money.Coins(0.003M))
                //.SendAsset(bitcoinToAddress, new AssetMoney(assetIdx, Convert.ToUInt64(amount)))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001M))
                .BuildTransaction(true);

            var ok = txBuilder.Verify(tx);
            Submit(tx);

            String hex = tx.ToHex();
            return hex;
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
            string url = "93.114.160.222:18333";
            //string url = "54.149.133.4:18333";
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
