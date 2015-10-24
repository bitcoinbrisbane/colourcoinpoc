using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        const String BobWIFKey = "cMdLBsUCQ92VSRmqfEL4TgJCisWpjVBd8GsP2mAmUZxQ9bh5E7CN";
        const String AliceWIFKey = "cPKW4EsFiPeczwHeSCgo4GTzm4T291Xb6sLGi1HoroXkiqGcGgsH";

        //n1aAcjdH4hbXXjeb2VWYCrKzLC6zEqvMnn
        //bXBY3ruScYBVDPyox57ciriEZfoHADfXbzC
        const String CarolWIFKey = "93FYFkYojan95DwW8SVJKwyf23W3pAnY2ZQKj7sQweP6qjzZ7i3";

        static void Main(string[] args)
        {
            const String BobAddress = "mxSimcis5yCPkBaFZ7ZrJ7fsPqLXatxTax";
            const String BobAssetAddress = "bX8Qc1nYCZT65cRjcbjg2wyaSjSWhY2gB5p";

            const String AliceAddress = "muJjaSHk99LGMnaFduU9b3pWHdT1ZRPASF";
            const String AliceAssetAddress = "bX5Gcpc75cdDxE2jcgXaLEuj5dEdBUdnpDu";
            
            BitcoinSecret alice = new BitcoinSecret(AliceWIFKey);
            BitcoinSecret bob = new BitcoinSecret(BobWIFKey);
            BitcoinSecret carol = new BitcoinSecret(CarolWIFKey);

            Console.WriteLine(carol.GetAddress().ToColoredAddress().ToString());

            //Check
            Debug.Assert(BobAddress == bob.GetAddress().ToString());
            Debug.Assert(AliceAddress == alice.GetAddress().ToString());

            Debug.Assert(BobAssetAddress == bob.GetAddress().ToColoredAddress().ToString());
            Debug.Assert(AliceAssetAddress == alice.GetAddress().ToColoredAddress().ToString());

            const String SILVER = "oLcV5F6R59zbChoTBQ962Har24tAnhbtHo";
            const String GOLD = "oWoYDF1Fxc7pSSoo97MMeH5NAmpqZhq5Ys";
            const String USD = "oSANrbK92PePSWkmjP7FtVqLtHWPwFTKWc";
            const String AUD = "oM4tzMCMyxQ5zgtb3QtPkFaoBtQKBG2WdP";

            //SIMPLE TRANSFERS
            //String result = Transfer(BobAddress, 4, AliceAddress, GOLD); //confirmed
            //String result = Transfer(AliceAddress, 10000, BobAddress, AUD); //confirmed
            //String result = Transfer(BobAddress, 1000, AliceAddress, USD); //confirmed
            //String result = Transfer(BobAddress, 15000, AliceAddress, USD); //confirmed
            //String result = Transfer(BobAddress, 100, AliceAddress, USD); //SENT
            //String result = Transfer(AliceAddress, 100, BobAddress, USD); //SENT
            //String result = Transfer(BobAddress, 100, AliceAddress, AUD); //SENT

            //new address
            //String result = Transfer(BobAddress, 100, carol.GetAddress().ToString(), SILVER); //CONFIRMED
            //String result = Transfer(BobAddress, 50000, carol.GetAddress().ToString(), USD); //CONFIRMED


            String result = Trade(BobAddress, 1, GOLD, carol.GetAddress().ToString(), 5000, USD);

            //BitcoinAssetId silverAsset = new BitcoinAssetId(SILVER, Network.TestNet);
            //BitcoinAssetId usdAsset = new BitcoinAssetId(SILVER, Network.TestNet);

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
            var fundingUTXO = txRepo.GetUnspent(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

            if (fundingUTXO == null)
            {
                throw new ArgumentNullException("Need more btc");
            }

            //Find unspent utxos with assets
            var assetFundingUtxos = txRepo.GetUnspent(fromAddress).Where(ux => ux.asset_id == assetId).ToList();

            //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            var ccutoxs = txRepo.GetTransactions(fromAddress);

            //Find new tx first
            var allOutputsForAsset = ccutoxs.OrderBy(a => a.block_time)
                .Where(i => i.outputs.Any(o => o.asset_id == assetId)) // && i.outputs.Any(oo => oo.addresses.Contains(fromAddress)))
                //.outputs.OrderByDescending(o => o.asset_quantity)
                //.outputs.FirstOrDefault(o => o.asset_id == assetId && o.asset_quantity >= amount && o.addresses.Contains(fromAddress));
                .ToList();

            //TODO:  CHANGE FOR DIVISIBILITY
            //Find output which contains an incoming asset
            var assetOutput = ccutoxs.OrderByDescending(a => a.block_time)
                .FirstOrDefault(i => i.outputs.Any(o => o.asset_id == assetId)) // && i.outputs.Any(oo => oo.addresses.Contains(fromAddress)))
                //.outputs.OrderByDescending(o => o.asset_quantity)
                //.outputs.FirstOrDefault(o => o.asset_id == assetId && o.asset_quantity >= amount && o.addresses.Contains(fromAddress));
                .outputs.FirstOrDefault(o => o.asset_id == assetId && o.asset_quantity >= amount);
            
            if (assetOutput == null)
            {
                throw new ArgumentNullException("Not enough assets");
            }

            ////Colour coin utxo that was sent
            var coin = new Coin(fromTxHash: new uint256(assetOutput.transaction_hash),
                fromOutputIndex: Convert.ToUInt32(assetOutput.index),
                amount: Money.Satoshis(assetOutput.value), //default fee
                scriptPubKey: bitcoinFromAddress.ScriptPubKey);

            //HACKE FOR 1 USD FROM ALICE BACK TO BOB.  TX 47~48
            //ADDRESS WAS ALICE
            //Colour coin utxo that was sent
            //var coin = new Coin(fromTxHash: new uint256("b52454d26d9020a241d3d3173270bf0b0e218fc4cf21d18c8ac34d8d29f0f988"),
            //    fromOutputIndex: 2,
            //    amount: Money.Satoshis(600), //default fee
            //    scriptPubKey: bitcoinFromAddress.ScriptPubKey);

            //Arbitary coin
            var forfees = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
                fromOutputIndex: fundingUTXO.output_index,
                amount: Money.Satoshis(fundingUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            BitcoinAssetId assetIdx = new BitcoinAssetId(assetId, Network.TestNet);
            //var recipient = NBitcoin.BitcoinAddress.Create(toAddress, NBitcoin.Network.TestNet);

            //44696
            ColoredCoin colored = coin.ToColoredCoin(assetIdx, Convert.ToUInt64(assetOutput.asset_quantity));
            //ColoredCoin colored = coin.ToColoredCoin(assetIdx, 44696);

            //FROM NIC
            var key1 = new BitcoinSecret(BobWIFKey);
            var key2 = new BitcoinSecret(CarolWIFKey);
            var txBuilder = new TransactionBuilder();

            var tx = txBuilder
                .AddKeys(key1, key2)
                .AddCoins(forfees, colored)
                .SendAsset(bitcoinToAddress, new AssetMoney(assetIdx, Convert.ToUInt64(amount)))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001m))
                .BuildTransaction(true);

            var ok = txBuilder.Verify(tx);
            var response = Submit(tx);

            return response;
        }


        public static string Trade(String fromAddress, Int64 fromAmount, String fromAssetId, String toAddress, Int64 toAmount, String toAssetId)
        {
            NBitcoin.Network network = NBitcoin.Network.TestNet;

            NBitcoin.BitcoinAddress bitcoinToAddress = new BitcoinAddress(toAddress, network);
            NBitcoin.BitcoinAddress bitcoinFromAddress = new BitcoinAddress(fromAddress, network);

            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);

            //Get a UTXO to fund.  Make sure its large enough, order by size.  Maybe order by date?
            var fundingUTXO = txRepo.GetUnspent(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();
            var fundingToUTXO = txRepo.GetUnspent(toAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();


            if (fundingUTXO == null | fundingToUTXO == null)
            {
                throw new ArgumentNullException("Need more btc");
            }

            //Find unspent utxos with assets
            var assetFromFundingUtxos = txRepo.GetUnspent(fromAddress).Where(ux => ux.asset_id == fromAssetId).ToList();
            var assetToFundingUtxos = txRepo.GetUnspent(toAddress).Where(ux => ux.asset_id == toAssetId).ToList();

            //Note last emmitting tx = 55ef4ea701ee0df5aac55d56a068d2488780da827aca3c08615cfa92dbfc470e
            //var fromCCutoxs = txRepo.GetTransactions(fromAddress);
            //var toCCutoxs = txRepo.GetTransactions(toAddress);


            ////Find new tx first (DEBUG ONLY)
            //var allOutputsForAsset = fromCCutoxs.OrderBy(a => a.block_time)
            //    .Where(i => i.outputs.Any(o => o.asset_id == fromAssetId)) // && i.outputs.Any(oo => oo.addresses.Contains(fromAddress)))
            //    //.outputs.OrderByDescending(o => o.asset_quantity)
            //    //.Where(o => o.asset_id == assetId && o.asset_quantity >= amount && o.addresses.Contains(fromAddress));
            //    .ToList();


            ////TODO:  CHANGE FOR DIVISIBILITY
            ////Find output which contains an incoming asset
            //var fromAssetOutput = fromCCutoxs.OrderByDescending(a => a.block_time)
            //    .FirstOrDefault(i => i.outputs.Any(o => o.asset_id == fromAssetId)) // && i.outputs.Any(oo => oo.addresses.Contains(fromAddress)))
            //    //.outputs.OrderByDescending(o => o.asset_quantity)
            //    //.outputs.FirstOrDefault(o => o.asset_id == assetId && o.asset_quantity >= amount && o.addresses.Contains(fromAddress));
            //    .outputs.FirstOrDefault(o => o.asset_id == fromAssetId && o.asset_quantity >= fromAmount);



            //FROM
            //todo, could be many
            var fromAssetOutput = assetFromFundingUtxos.SingleOrDefault(u => u.asset_quantity >= fromAmount);
            var toAssetOutput = assetToFundingUtxos.SingleOrDefault(u => u.asset_quantity >= toAmount);

            if (fromAssetOutput == null | toAssetOutput == null)
            {
                throw new ArgumentNullException("Not enough assets");
            }

            //TODO:  NEED MULTIPLE COINS
            //DO THE FROM
            ////Colour coin utxo that was sent
            var fromCoin = new Coin(fromTxHash: new uint256(fromAssetOutput.transaction_hash),
                //fromOutputIndex: Convert.ToUInt32(fromAssetOutput.index),
                fromOutputIndex: Convert.ToUInt32(fromAssetOutput.output_index),
                amount: Money.Satoshis(fromAssetOutput.value), //default fee
                scriptPubKey: bitcoinFromAddress.ScriptPubKey);

            BitcoinAssetId fromAssetIdx = new BitcoinAssetId(fromAssetId, Network.TestNet);
            
            ColoredCoin fromColored = fromCoin.ToColoredCoin(fromAssetIdx, Convert.ToUInt64(fromAssetOutput.asset_quantity));


            ////TODO:  NEED MULTIPLE COINS
            //var toCoin = new Coin(fromTxHash: new uint256(toAssetOutput.transaction_hash),
            //    fromOutputIndex: Convert.ToUInt32(toAssetOutput.output_index),
            //    amount: Money.Satoshis(toAssetOutput.value), //default fee
            //    scriptPubKey: bitcoinToAddress.ScriptPubKey);

            BitcoinAssetId toAssetIdx = new BitcoinAssetId(toAssetId, Network.TestNet);
            ColoredCoin toColored = MakeColouredCoin(toAssetOutput, toAssetId, bitcoinToAddress.ScriptPubKey);


            //Arbitary coin
            var fromFeeCoin = new Coin(fromTxHash: new uint256(fundingUTXO.transaction_hash),
                fromOutputIndex: fundingUTXO.output_index,
                amount: Money.Satoshis(fundingUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingUTXO.script_hex)));

            var toFeeCoin = new Coin(fromTxHash: new uint256(fundingToUTXO.transaction_hash),
                fromOutputIndex: fundingToUTXO.output_index,
                amount: Money.Satoshis(fundingToUTXO.value), //20000
                scriptPubKey: new Script(Encoders.Hex.DecodeData(fundingToUTXO.script_hex)));



            //FROM NIC
            var key1 = new BitcoinSecret(BobWIFKey);
            var key2 = new BitcoinSecret(CarolWIFKey);
            var txBuilder = new TransactionBuilder();

            var tx = txBuilder
                .AddKeys(key1, key2)
                .AddCoins(fromFeeCoin, fromColored)
                .SendAsset(bitcoinToAddress, new AssetMoney(fromAssetIdx, Convert.ToUInt64(fromAmount)))
                .SetChange(bitcoinFromAddress)
                .SendFees(Money.Coins(0.001m))
                .Then()
                //.AddKeys(key1, key2)
                .AddCoins(toFeeCoin, toColored)
                .SendAsset(bitcoinFromAddress, new AssetMoney(toAssetIdx, Convert.ToUInt64(toAmount)))
                .SetChange(bitcoinToAddress)
                .SendFees(Money.Coins(0.001m))
                .BuildTransaction(true);

            var ok = txBuilder.Verify(tx);
            var response = Submit(tx);

            return response;
        }

        public void GetUtxo(String assetAddress, String assetId, Int64 amout)
        {
            //UTXOS
            CoinPrism txRepo = new CoinPrism(true);
            var fundingUtxos = txRepo.GetUnspent(assetAddress).Where(ux => ux.asset_id == assetId).ToList();
        }

        public static ColoredCoin MakeColouredCoin(Output output, String assetId, Script scriptPubKey)
        {
            return MakeColouredCoin(output.output_hash, assetId, Convert.ToUInt32(output.index), output.value, scriptPubKey, Convert.ToUInt64(output.asset_quantity));
        }

        public static ColoredCoin MakeColouredCoin(UnspentTransactionResponse utxo, String assetId, Script scriptPubKey)
        {
            return MakeColouredCoin(assetId, utxo.transaction_hash, Convert.ToUInt32(utxo.output_index), utxo.value, scriptPubKey, Convert.ToUInt64(utxo.asset_quantity));
        }

        public static ColoredCoin MakeColouredCoin(String assetId, String txhash, UInt32 output_index, Int64 output_value, Script scriptPubKey, UInt64 asset_quantity)
        {
            //TODO:  NEED MULTIPLE COINS
            //DO THE FROM
            ////Colour coin utxo that was sent
            var fromCoin = new Coin(fromTxHash: new uint256(txhash),
                //fromOutputIndex: Convert.ToUInt32(fromAssetOutput.index),
                fromOutputIndex: Convert.ToUInt32(output_index),
                amount: Money.Satoshis(output_value), //default fee
                scriptPubKey: scriptPubKey);

            BitcoinAssetId bitcoinAssetId = new BitcoinAssetId(assetId, Network.TestNet);
            ColoredCoin coin = fromCoin.ToColoredCoin(bitcoinAssetId, asset_quantity);

            return coin;
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
            var fundingUTXO = txRepo.GetUnspent(bitcoinFromAddress.ToString()).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

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
            var fundingUTXO = txRepo.GetUnspent(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

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
            var fundingUTXO = txRepo.GetUnspent(fromAddress).Where(ux => ux.value > 10000).OrderByDescending(o => o.value).First();

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

        public static String Submit(Transaction tx)
        {

            //Info.Blockchain.API.PushTx.PushTx bcInfo = new Info.Blockchain.API.PushTx.PushTx();
            //http://btc.blockr.io/api/v1/tx/push


            var cli = new WebClient();
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";

            RawTx t = new RawTx();
            t.hex = tx.ToHex();

            String json = Newtonsoft.Json.JsonConvert.SerializeObject(t);

            String response = cli.UploadString("http://tbtc.blockr.io/api/v1/tx/push", json);


            //IList<String> nodes = new List<String>(3);
            //nodes.Add("54.149.133.4:18333");
            ////SOME TEST NET NODES
            ////54.149.133.4:18333
            ////52.69.206.155:18333
            ////93.114.160.222:18333
            //string url = "93.114.160.222:18333"; //nic
            ////url = "52.4.156.236:18333";
            ////string url = "54.149.133.4:18333";


            //using (var node = NBitcoin.Protocol.Node.ConnectToLocal(NBitcoin.Network.TestNet))
            //{
            //    node.VersionHandshake();

            //    NBitcoin.Transaction[] transactions = new Transaction[1];
            //    transactions[0] = tx;

            //    node.SendMessage(new NBitcoin.Protocol.InvPayload(transactions));
            //    node.SendMessage(new NBitcoin.Protocol.TxPayload(tx));
            //}

            //using(var node = NBitcoin.Protocol.Node.Connect(NBitcoin.Network.TestNet, url))
            //{
            //    node.VersionHandshake();

            //    System.Threading.Thread.Sleep(100);

            //    NBitcoin.Transaction[] transactions = new Transaction[1];
            //    transactions[0] = tx;

            //    node.SendMessage(new NBitcoin.Protocol.InvPayload(transactions));
            //    node.SendMessage(new NBitcoin.Protocol.TxPayload(tx));
            //}

            var r = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(response);

            return r.data;

        }
    }
}
