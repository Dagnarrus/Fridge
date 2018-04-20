namespace Fridge.Messaging
{
    using System;
    using System.Threading.Tasks;

    public interface IMessageBus
    {
        /// <summary>
        /// Subscribes a specified hadnler to all messages of the given type. 
        /// </summary>
        /// <typeparam name="TMessage">The type of message to subscribe.</typeparam>
        /// <param name="messageHandler">The handler to execute on the message object.</param>
        /// <param name="errorHandler">An error handler to execute if any unhandled exception occurs in the message handler.</param>
        /// <param name="scheduler">A task scheduler to execute the handler within a specific thread or threadpool.</param>
        void Subscribe<TMessage>(Action<TMessage> messageHandler, Action<Exception> errorHandler = null, TaskScheduler scheduler = null);
        /// <summary>
        /// Unsubscribes the given handler from any further messages of the message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of message to unsubscribe.</typeparam>
        /// <param name="messageHandler">The handle being unsubscribed.</param>
        void Unsubscribe<TMessage>(Action<TMessage> messageHandler);
        /// <summary>
        /// Publishes the message to all subscribe consumers of the given type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="message">The message to be published.</param>
        void Publish<TMessage>(TMessage message);
    }
}
