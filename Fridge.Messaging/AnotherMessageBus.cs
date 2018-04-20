namespace Fridge.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Fridge.Messaging.Tasks;

    public class AnotherMessageBus : IMessageBus
    {
        private readonly List<ConsumerInfo> consumers = new List<ConsumerInfo>();

        private static readonly object syncLock = new object();

        private static readonly Lazy<AnotherMessageBus> lazy = new Lazy<AnotherMessageBus>(() => new AnotherMessageBus());
        
        private AnotherMessageBus() { }

        public static IMessageBus Bus => lazy.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageHandler"></param>
        /// <param name="errorHandler"></param>
        /// <param name="scheduler"></param>
        public void Subscribe<TMessage>(Action<TMessage> messageHandler, Action<Exception> errorHandler = null, TaskScheduler scheduler = null)
        {
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            if (errorHandler == null)
                errorHandler = this.NotifyUnhandledError;

            SafeSubscribe(new ConsumerInfo<TMessage>()
            {
                MessageHandler = messageHandler, 
                ErrorHandler = errorHandler, 
                Scheduler = scheduler
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="messageHandler"></param>
        public void Unsubscribe<TMessage>(Action<TMessage> messageHandler)
        {
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            SafeUnsubscribe(messageHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        public void Publish<TMessage>(TMessage message)
        {
            SafePublish(message);
        }


        private void SafeSubscribe(ConsumerInfo consumer)
        {
            lock (syncLock)
            {
                this.consumers.Add(consumer);
            }
        }

        private void SafeUnsubscribe<TMessage>(Action<TMessage> messageHandler)
        {
            lock (syncLock)
            {
                var consumer = consumers.FirstOrDefault(c => (c as ConsumerInfo<TMessage>)?.MessageHandler == messageHandler);

                if (consumer != null)
                    consumers.Remove(consumer);
            }
        }

        // TODO: decide whether iteration for execution needs to be performed within a lock.  First assumption is yes.
        // The method will only need to hold the lock long enough to generate the tasks.

        private void SafePublish<TMessage>(TMessage message)
        {
            lock (syncLock)
            {
                foreach (var consumer in this.consumers.OfType<ConsumerInfo<TMessage>>())
                {
                    TaskExecutor.Execute(
                        () => consumer.MessageHandler(message), 
                        consumer.ErrorHandler, 
                        consumer.Scheduler
                    );
                }
            }
        }

        private void NotifyUnhandledError(Exception exception)
        {
            // This is going to be similar to the SafePublish. But we will need to iterate the collection outside of the lock, 
            // and allow the default error continuation to action for unhandled errors.
            foreach (var consumer in this.consumers.OfType<ConsumerInfo<Exception>>())
            {
                TaskExecutor.Execute(
                    () => consumer.MessageHandler(exception),
                    null,
                    consumer.Scheduler
                );
            }
        }
    }
}
