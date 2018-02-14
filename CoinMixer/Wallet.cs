using System;
using System.Collections.Generic;
using System.Text;

namespace CoinMixer
{
    /*
     * Wallet is responsible to generate annonymous account for every user account known 
     * to outside world. When receiving money, recipient discloses user account to sender
     * Mixer uses the generated accounts to hide the actual recipients from sender.
     * 
     * wallet can also hold two-factor or multi-factor signature ID of the user
     */
    public class Wallet
    {
        public readonly string UserAccount;
        private readonly string GeneratedAccount1;
        private readonly string GeneratedAccount2;
        private readonly string GeneratedAccount3;
        private int count = 0;
        public Wallet(string userAccount)
        {
            UserAccount = userAccount;
            GeneratedAccount1 = System.Guid.NewGuid().ToString();
            GeneratedAccount2 = System.Guid.NewGuid().ToString();
            GeneratedAccount3 = System.Guid.NewGuid().ToString();
        }

        public string GetRemittanceAddress()
        {
            switch (++count % 3)
            {
                case 0:
                    count = 0;
                    return GeneratedAccount3;
                case 1: return GeneratedAccount2;
                case 2: return GeneratedAccount1;
            }
            return GeneratedAccount2;
        }
    }
}
