using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace FFF.Console.Out
{
    public static class WriteLine
    {
        private static IDisposable _observableConsoleWriteLine;

        public static void Subscribe(Action callBack, TimeSpan timeInterval) =>
            InternalSubscribe(() => callBack?.Invoke(), timeInterval);

        public static void AsyncSubscribe(Action callBack, TimeSpan timeInterval) =>
            InternalSubscribe(() => callBack?.BeginInvoke(null, null), timeInterval);

        public static void Subscribe(Action callBack) =>
            InternalSubscribe(() => callBack?.Invoke());

        public static void AsyncSubscribe(Action callBack) =>
            InternalSubscribe(() => callBack?.BeginInvoke(null, null));

        private static void InternalSubscribe(Action callBack)
        {
            // Clear
            Dispose();
            //The input sequence. Produces values potentially quicker than consumer
            //Project the event you receive, into the result of the async method
            _observableConsoleWriteLine = Observable.FromAsync(() => System.Console.Out.WriteLineAsync())
               //Ensure that the results are serialized
               .Repeat()
               .Publish()
               .RefCount()
               .SubscribeOn(Scheduler.Default)
               //do what you will here with the results of the async method calls
               .Subscribe(_ => callBack());
        }

        private static void InternalSubscribe(Action callBack, TimeSpan timeInterval)
        {
            // Clear
            Dispose();
            //The input sequence. Produces values potentially quicker than consumer
            //Project the event you receive, into the result of the async method
            _observableConsoleWriteLine = Observable.Interval(timeInterval)
                //Project the event you receive, into the result of the async method
                .Select(_ => Observable.FromAsync(() => System.Console.Out.WriteLineAsync()))
                //Ensure that the results are serialized
                .Concat()
                //do what you will here with the results of the async method calls
                .Subscribe(_ => callBack());
        }

        public static void Dispose() =>
            _observableConsoleWriteLine?.Dispose();
    }
}
