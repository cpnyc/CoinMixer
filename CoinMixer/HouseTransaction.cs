using System;

namespace CoinMixer
{
    /*
     * HouseTransaction class is used for transaction occuring 
     * from _HOUSE_ACCT_ to Recipient
     */
    public class HouseTransaction
    {
        public Double Amount;
        public Double TotalAmount;
        public Wallet RecipientWallet;
        public string FromAddress;
        public DateTime Timestamp;
    }
}