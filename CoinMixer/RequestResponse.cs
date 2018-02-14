using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoinMixer
{
    /*
     * RequestResponse class
     * This class manages communication for POST/GET to obtain transactions and send transactions
     * 
     */
    public class RequestResponse
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Assembly.GetEntryAssembly(), Logger.RequestResponse);

        // this url will be used for polling bitcoin/crypto network to read inbound transaction
        // for example, person A wants to send money to person B
        private readonly string httpUrlToPollTransaction = "http://url";

        // this url will be used for sending outbound transaction to recipient wallet
        // for example, person B is going to recieve money to its wallet (money was sent by person A)
        private readonly string httpUrlToSendCoinToRecipientWallet = "http://url";

        private readonly CancellationTokenSource _ctx;
        private TransactionProcessor _tp;

        public RequestResponse(CancellationTokenSource ctx, TransactionProcessor tp)
        {
            _ctx = ctx;
            _tp = tp;
        }
        /*
         * This method will send money to actual recipient's wallet in JSON format
         */
        public void SendTransaction(Transaction tran)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrlToSendCoinToRecipientWallet);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(tran);
                Log.DebugFormat("Outbout Transaction => {0}",json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
        /*
         * This method will poll transactions happening on bitcoin network
         * and process them asynchronously.
         */
        public async void PollTransactions()
        {
            if(_ctx == null) return;

            while (_ctx.IsCancellationRequested == false)
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var jsonStr = httpClient.GetStringAsync(httpUrlToPollTransaction).Result;
                        Log.DebugFormat("Inbound TransactionList => {0}", jsonStr);
                        var allTransactions = JsonConvert.DeserializeObject<Transaction[]>(jsonStr);
                        _tp.ProcessTransactions(allTransactions);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("Error fetching transactions", e);
                    }
                }

                //10 seconds sleep and then poll for newer transactions...
                //this can be replaced by listening to events on bitcoin network
                await Task.Delay(10000);
            }
        }
    }
}
