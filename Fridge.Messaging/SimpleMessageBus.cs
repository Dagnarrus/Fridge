namespace Fridge.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SimpleMessageBus : IMessageBus
    {
        //TODO decide whether the handlers should be shared, or per instance...
        private static readonly List<ConsumerInfo> consumers = new List<ConsumerInfo>();


        /// <summary>
        /// Subscribes a specified handler to all messages of the given type. 
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe.</typeparam>
        /// <param name="messageHandler">The handler to execute on the message object.</param>
        /// <param name="errorHandler">An error handler to execute if any unhandled exception occurs in the message handler.</param>
        /// <param name="scheduler">A task scheduler to execute the handler within a specific thread or threadpool.</param>
        public void Subscribe<TMessage>(Action<TMessage> messageHandler, Action<Exception> errorHandler = null, TaskScheduler scheduler = null)
        {
            //TODO decide if the error should state that the object, whose action was referenced is null.
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            consumers.Add(new ConsumerInfo<TMessage>
            {
                MessageHandler = messageHandler, 
                ErrorHandler = errorHandler, 
                Scheduler = scheduler
            });
        }

        /// <summary>
        /// Unsubscribes the given handler from any further messages of the message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to unsubscribe.</typeparam>
        /// <param name="messageHandler">The handle being unsubscribed.</param>
        public void Unsubscribe<TMessage>(Action<TMessage> messageHandler)
        {
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            var consumer = consumers.FirstOrDefault(cnsmr =>
            {
                var c = cnsmr as ConsumerInfo<TMessage>;

                return c != null && c.MessageHandler == messageHandler;
            });

            if (consumer != null)
                consumers.Remove(consumer);
        }

        /// <summary>
        /// Publishes the message to all subscribe consumers of the given type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="message">The message to be published.</param>
        public void Publish<TMessage>(TMessage message)
        {
            foreach (var consumer in consumers.OfType<ConsumerInfo<TMessage>>())
                Execute(consumer, message);
        }
        
        private static void Execute<TMessage>(ConsumerInfo<TMessage> consumer, TMessage message)
        {
            try
            {
                consumer.MessageHandler(message);
            }
            catch (Exception ex)
            {
                consumer.ErrorHandler?.Invoke(ex);
            }
        }
    }
}
