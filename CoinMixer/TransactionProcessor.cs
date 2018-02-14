using System;
using System.Collections.Generic;
using System.Text;

namespace CoinMixer
{
    /*
     * TransactionProcessor obtains all transactions from website and
     * sends to mixer to further process. TransactionProcessor also keeps track of 
     * last processed transaction.
     */
    public class TransactionProcessor
    {
        private DateTime _lastTransactionDateTime = DateTime.MinValue;
        private Transaction _lastTransaction = new Transaction();
        
        protected readonly EventQueue<Transaction> TransactionQueue;

        public TransactionProcessor(Action<Transaction> onNewTransaction)
        {
            TransactionQueue = new EventQueue<Transaction>(onNewTransaction);
        }

        public void ProcessTransactions(Transaction[] listOfTransactions)
        {
            foreach (var tran in listOfTransactions)
            {
                if (DateTime.TryParse(tran.timestamp, out DateTime tranTime))
                {
                    if (_lastTransactionDateTime < tranTime)
                    {
                        // process the new transaction 
                        // Send to event queue and process in separate thread
                        TransactionQueue.Insert(tran);
                        // save the last transaction
                        _lastTransaction = tran;
                        _lastTransactionDateTime = tranTime;
                    }
                    else if (_lastTransactionDateTime.Equals(tranTime))
                    {
                        // compare all values of transaction to ensure that the transaction was processed earlier
                        if( tran.timestamp == _lastTransaction.timestamp &&
                            tran.amount == _lastTransaction.amount &&
                            tran.fromAddress == _lastTransaction.fromAddress &&
                            tran.toAddress == _lastTransaction.toAddress)
                            continue;

                        // process the new transaction 
                        // Send to event queue and process in separate thread
                        TransactionQueue.Insert(tran);
                        // save the last transaction
                        _lastTransaction = tran;
                        _lastTransactionDateTime = tranTime;
                    }
                }
            }
        }
    }
}
