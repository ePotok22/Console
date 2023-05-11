using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace FFF.Console.In
{
    public static class ReadLine
    {
        private static IDisposable _observableConsoleReadLine;

        public static void Subscribe(Action<string> callBack, TimeSpan timeInterval) =>
            InternalSubscribe(_ => { if (_ != null) callBack?.Invoke(_); }, timeInterval);

        public static void SubscribeAsync(Action<string> callBack, TimeSpan timeInterval) =>
            InternalSubscribe(_ => { if (_ != null) callBack?.BeginInvoke(_, null, null); }, timeInterval);

        public static void Subscribe(Action<string> callBack) =>
            InternalSubscribe(_ => { if (_ != null) callBack?.Invoke(_); });

        public static void SubscribeAsync(Action<string> callBack) =>
            InternalSubscribe(_ => { if (_ != null) callBack?.BeginInvoke(_, null, null); });

        private static void InternalSubscribe(Action<string> callBack)
        {
            // Clear
            Dispose();
            //The input sequence. Produces values potentially quicker than consumer
            //Project the event you receive, into the result of the async method
            _observableConsoleReadLine = Observable.FromAsync(() => System.Console.In.ReadLineAsync())
               //Ensure that the results are serialized
               .Repeat()
               .Publish()
               .RefCount()
               .SubscribeOn(Scheduler.Default)
               //do what you will here with the results of the async method calls
               .Subscribe(callBack);
        }

        private static void InternalSubscribe(Action<string> callBack, TimeSpan timeInterval)
        {
            // Clear
            Dispose();
            //The input sequence. Produces values potentially quicker than consumer
            //Project the event you receive, into the result of the async method
            _observableConsoleReadLine = Observable.Interval(timeInterval)
                //Project the event you receive, into the result of the async method
                .Select(_ => Observable.FromAsync(() => System.Console.In.ReadLineAsync()))
                //Ensure that the results are serialized
                .Concat()
                //do what you will here with the results of the async method calls
                .Subscribe(callBack);
        }

        public static void Dispose() =>
             _observableConsoleReadLine?.Dispose();
    }
}
