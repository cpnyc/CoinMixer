using System;
using System.Collections.Generic;
using System.Text;

namespace CoinMixer
{
    /*
     * Transaction is communication object to transfer information in JSON format
     * 1) to read transaction by polling bitcoin network
     * 2) send money to recipient's wallet over internet/network
     */
    [Serializable]
    public class Transaction
    {
        public string timestamp { get; set; }
        public string fromAddress { get; set; }
        public string toAddress { get; set; }
        public string amount { get; set; }
    }
}
