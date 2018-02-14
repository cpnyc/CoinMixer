using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CoinMixer
{
    /*
     * Mixer class
     * Mixer keeps mapping of user account and its anonymous wallet (generated accounts)
     * 
     * Mixer moves funds 
     * 1) from sender to house account
     * 2) from house account to anonymous wallet of the recipient in two random parts
     * 
     * When transfer is requested to move money from sender to recipient in amount of 10,
     * mixer will move 10.0 to house account and then send two payments e.g. 6.455 and 3.545
     * to recipient's secret address (as mentioned in wallet)
     * 
     * Mixer also charges 0.01% mixing fee rounded upto 4 decimal points of the number 
     * for every transfer of funds from house account to recipient account
     * 
     */
    public class Mixer
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Assembly.GetEntryAssembly(), Logger.Mixer);

        // (userAccount => Wallet)
        protected Dictionary<string, Wallet> AccountWalletMap = new Dictionary<string, Wallet>();

        // house account
        protected const string _houseAddress = "_HOUSE_ACCT_";

        // mixer account 
        protected const string _mixerAddress = "_MIXER_ACCT_";

        // house account value
        protected Double _houseAccount = 0.0;

        // mixer account
        protected Double _mixerAccount = 0.0;

        private const Double _mixingFeeMultiplier = (Double) 0.01/100;
        protected RequestResponse requestResponse = new RequestResponse(null, null);
        // to syncronize house account
        private static readonly Object _lockHouseAccount = new Object();
        private static readonly Object _lockMixerAccount = new Object();

        // event queue to process randomize/partial payout to recipient account 
        protected EventQueue<HouseTransaction> MixerQueue;

        //randomizer of amount
        protected Random randomizer = new Random();

        public Mixer(List<Wallet> walletList)
        {
            MixerQueue = new EventQueue<HouseTransaction>(OnMixer);
            foreach (var wallet in walletList)
            {
                AccountWalletMap.Add(wallet.UserAccount, wallet);
            }
        }


        public void OnMixer(HouseTransaction tran)
        {
            // set up randomized partial fund (in partial amounts) from house account to recipient
            //var userAccount = AccountMap[tran.RecipientWallet];

            /*
             * This logic sends creates two small payments to recipients 
             * For example, Alice sent 12.5 to Bob
             * House account will send (randomized two payments) 7.5 and 5.0 to Bob's generated account
             * 
             */
            double newAmtToTransfer = tran.Amount;
            if (tran.Amount.Equals(tran.TotalAmount))
            {
                int r = randomizer.Next(100);
                double multiplier = (double) r / 100;
                newAmtToTransfer = tran.TotalAmount * multiplier;
            }

            lock (_lockHouseAccount)
            {
                _houseAccount -= newAmtToTransfer;
            }
            var tranTime = DateTime.Now;

            var remainingAmount = tran.Amount - newAmtToTransfer;
            if (newAmtToTransfer > 0)
            {
                var mixingFee = Math.Round(_mixingFeeMultiplier * newAmtToTransfer, 4, MidpointRounding.AwayFromZero);
                lock (_lockMixerAccount)
                {
                    _mixerAccount += mixingFee;
                    newAmtToTransfer -= mixingFee;
                    Log.DebugFormat("Mixing fee charged {0} to recipient {1}", mixingFee, tran.RecipientWallet.UserAccount);
                }
                Log.DebugFormat("Amount {0} transfered from HouseAccount to Recipient {1}", newAmtToTransfer, tran.RecipientWallet.UserAccount);

                var postTran = new Transaction();
                postTran.amount = newAmtToTransfer.ToString();
                postTran.fromAddress = tran.FromAddress;
                postTran.toAddress = tran.RecipientWallet.GetRemittanceAddress();
                postTran.timestamp = tranTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                requestResponse.SendTransaction(postTran);
            }
            // send fund/amount from house account to generated account
            if (remainingAmount > 0)
            {
                HouseTransaction newTran = new HouseTransaction();
                newTran.Amount = remainingAmount;
                newTran.TotalAmount = tran.TotalAmount;
                newTran.FromAddress = _houseAddress;
                newTran.RecipientWallet = tran.RecipientWallet;
                newTran.Timestamp = tranTime;
                MixerQueue.Insert(newTran);
            }
            
        }

        public void OnNewTransaction(Transaction tran)
        {
            // find generated address from "toAddress" field
            Wallet userWallet = AccountWalletMap[tran.toAddress];

            // return if wallet not found
            if (userWallet == null) return;

            // this transaction is created by UI, so ignore it
            if (string.IsNullOrWhiteSpace(tran.fromAddress) || _houseAddress.Equals(tran.fromAddress)) 
                return;
            // send money to house account
            // increment house account value by adding new coins
            if (Double.TryParse(tran.amount, out Double amt))
            {
                lock (_lockHouseAccount)
                {
                    _houseAccount += amt;
                }
                var tranTime = DateTime.Now;
                Log.DebugFormat("Amount {0} transfered from Sender {1} to HouseAccount", tran.amount, tran.fromAddress);
                var postTran = new Transaction();
                postTran.amount = tran.amount; 
                postTran.fromAddress = tran.fromAddress;  // use srcAddr if sender needs to be unknown on the transaction
                postTran.toAddress = _houseAddress;
                postTran.timestamp = tranTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                requestResponse.SendTransaction(postTran);
                
                // send fund/amount from house account to generated account
                HouseTransaction newTran = new HouseTransaction();
                newTran.Amount = amt;
                newTran.TotalAmount = amt;
                newTran.FromAddress = _houseAddress;
                newTran.RecipientWallet = userWallet;
                newTran.Timestamp = tranTime;
                MixerQueue.Insert(newTran);
            }
        }
    }
}
