namespace Fridge.Messaging.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // TODO decide whether this class should be public. If public, perhaps move into an async project?
    internal class TaskExecutor
    {
        private static readonly Action<Task> DefaultErrorContinuation = task =>
        {
            try
            {
                task.Wait();
            }
            catch { }
        };

        public static void Execute(Action action, Action<Exception> errorHandler = null, TaskScheduler scheduler = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (scheduler == null)
                scheduler = TaskScheduler.Current;

            var task = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, scheduler);

            if (errorHandler == null)
            {
                task.ContinueWith(
                    DefaultErrorContinuation,
                    TaskContinuationOptions.OnlyOnFaulted |
                    TaskContinuationOptions.ExecuteSynchronously
                );
            }
            else
            {
                task.ContinueWith(
                    t => errorHandler(t.Exception.GetBaseException()),
                    TaskContinuationOptions.OnlyOnFaulted |
                    TaskContinuationOptions.ExecuteSynchronously
                );
            }
        }
    }
}
