namespace Fridge.Messaging
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ConsumerInfo { }

    internal class ConsumerInfo<TMessage> : ConsumerInfo
    {
        public Action<TMessage> MessageHandler { get; set; }

        public Action<Exception> ErrorHandler { get; set; }

        public TaskScheduler Scheduler { get; set; }
    }
}
