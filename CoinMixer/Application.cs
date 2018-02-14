using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

namespace CoinMixer
{
    /*
     * Main application class for entry point
     * Application class configures following objects:
     *   Mixer (coin mixer)
     *   Wallet (user wallet)
     *   RequestResponse (communication to gemini website)
     *   TransactionProcessor (Get all transactions from website and process them)
     *   Logger (uses log4net to log information/debug into a log file)
     */
    public class Application
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var ctx = new CancellationTokenSource();
            List<Wallet> walletList = new List<Wallet>();
            walletList.Add(new Wallet("Alice"));
            walletList.Add(new Wallet("Bob"));
            walletList.Add(new Wallet("Test1"));

            Mixer mixer = new Mixer(walletList);
            TransactionProcessor tp = new TransactionProcessor(mixer.OnNewTransaction);
            RequestResponse rr = new RequestResponse(ctx, tp);

            Task t = Task.Factory.StartNew(rr.PollTransactions, ctx.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            
            Console.WriteLine("Press any key to end this coin mixer program...");
            Console.ReadKey();
            ctx.Cancel();
            t.Wait(ctx.Token);
        }
    }


}
