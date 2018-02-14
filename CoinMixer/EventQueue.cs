using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Repository;

namespace CoinMixer
{

    /*
     * EventQueue class
     * This class constructor takes function pointer a.k.a. delegate as input 
     * When an item is inserted in a queue, it is immediately dequeued and 
     * processed by the delegate provided. This item is processed on the 
     * separate thread than the caller thread. This provides scalability
     * in the program.
     */
    public class EventQueue<T> : IDisposable
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Assembly.GetEntryAssembly(), Logger.EventQueue);

        protected readonly BlockingCollection<T> MyQueue = new BlockingCollection<T>();
        protected readonly CancellationTokenSource TokenSource;
        protected readonly CancellationToken Token;
        protected Task LongRunningTask;

        protected Action<T> MyAction;
        protected bool StopThread;
        protected string Name;

        public EventQueue(Action<T> action)
        {
            MyAction = action;
            Name = action.ToString();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            LongRunningTask = Task.Factory.StartNew(ProcessItem, Token);
            
        }

        public void SetAction(Action<T> action)
        {
            MyAction = action;
            Name = action.ToString();
            //  Log.DebugFormat("EventQueue setting new action {0}", Name);
        }

        public virtual void Insert(T item)
        {
            if (!StopThread && item != null)
                MyQueue.Add(item, Token);
        }

        protected virtual void ProcessItem()
        {
            while (!StopThread)
            {
                try
                {
                    T item = MyQueue.Take();
                    if (item == null)
                        continue;

                    if (MyAction != null)
                    {
                        try
                        {
                            MyAction(item);
                        }
                        catch (Exception e)
                        {
                            Log.Error(String.Format("ERROR: Failed to process item.\nItem=>{0}", item), e);
                        }
                    }
                    else
                    {
                        Log.Error(
                            "Action cannot be null. Use parameterized constructor or SetAction method to fix this problem.");
                        StopThread = true;
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // operation has been canceled
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR: Failed to dequeue item.", ex);
                }
            }
        }

        public bool IsEmpty
        {
            get { return MyQueue.Count == 0; }
        }

        public virtual void Dispose()
        {
            StopThread = true;
            if (TokenSource != null && !TokenSource.IsCancellationRequested) TokenSource.Cancel();
            if (MyQueue.IsCompleted == false)
            {
                try
                {
                    MyQueue.Add(default(T));
                    MyQueue.CompleteAdding();
                }
                catch (InvalidOperationException e)
                {
                    // ignore this exception
                }
            }
        }

    }
}