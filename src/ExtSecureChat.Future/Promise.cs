using ExtSecureChat.Future.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExtSecureChat.Future
{
    public class Promise<T>
    {
        private const int TTL = 50000;
        private const int WAIT_TIME = 250;

        private int currentTTL = 0;

        private bool Completed;
        private bool Failed;
        private bool Cancelled;

        public Exception Exception { get; private set; }

        private Task<T> executorTask;
        private Task resolveTask;
        private Task rejectTask;

        private Func<T> executorFunc;
        private Action<T> resolveFunc;
        private Action<Exception> rejectFunc;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <example>
        /// var promise = new Promise<string>(() =>
        /// {
        ///     return "You are cool";
        /// });
        /// </example>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute</param>
        public Promise(Func<T> executor)
        {
            InitializePromise(executor);
        }

        private void InitializePromise(Func<T> executor)
        {
            executorFunc = executor;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executorTask = new Task<T>(() =>
            {
                try
                {
                    return TaskExecute();
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    rejectTask.Start();
                    return default(T);
                }
            }, cancellationTokenSource.Token);

            var waitTask = new Task(() =>
            {
                while ((resolveFunc == null || resolveTask == null) && (rejectFunc == null || rejectTask == null))
                {
                    if (currentTTL > TTL)
                    {
                        throw new PromiseTimoutException(String.Format("Promise exceeded the TTL of: {0}", TTL));
                    }

                    Thread.Sleep(WAIT_TIME);
                    currentTTL++;
                }

                executorTask.Start();
            });

            waitTask.Start();
        }

        #region --- Executor Promise Methods ---

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own)
        /// </summary>
        /// <example>
        /// var promise = new Promise<string>((resolve, reject) =>
        /// {
        ///     if (iAmCool())
        ///     {
        ///         resolve("You are cool!");
        ///     }
        ///     else
        ///     {
        ///         reject("You are not cool :(");
        ///     }
        /// });
        /// </example>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action<dynamic>, Action<string>> executor)
        {
            dynamic resolved = null;
            Exception exception = null;

            InitializePromise(() =>
            {
                var execTask = Task.Factory.StartNew(() => {
                    executor(
                        (resolve) =>
                        {
                            resolved = resolve;
                        },
                        (reject) =>
                        {
                            exception = new PromiseRejectException(reject);
                        }
                    );
                });

                while (resolved == null && exception == null)
                {
                    if (currentTTL > TTL)
                    {
                        throw new PromiseTimoutException(String.Format("Promise exceeded the TTL of: {0}", TTL));
                    }

                    Thread.Sleep(WAIT_TIME);
                    currentTTL++;
                }

                if (exception != null)
                {
                    throw exception;
                }

                return resolved;
            });
        }

        #endregion

        public Promise<T> Then(Action<T> resolve)
        {
            resolveFunc = resolve;

            resolveTask = executorTask.ContinueWith(t =>
            {
                thenExecute(t.Result);
            }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);

            return this;
        }

        public Promise<T> Catch(Action<Exception> reject)
        {
            rejectFunc = reject;

            rejectTask = new Task(() =>
            {
                catchExecute(Exception.InnerException);
            });

            return this;
        }

        public void Wait()
        {
            executorTask?.Wait();
            resolveTask?.Wait();
            if (Failed || (rejectTask != null && !rejectTask.IsCompleted))
            {
                rejectTask?.Wait();
            }
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            Cancelled = true;
        }

        private T TaskExecute()
        {
            cancellationToken.ThrowIfCancellationRequested();

            var t = Task.Factory.StartNew(() =>
            {
                return executorFunc.Invoke();
            });

            while (!t.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            if (resolveFunc == null)
            {
                Completed = true;
            }

            return t.Result;
        }

        private void thenExecute(T response)
        {
            resolveFunc?.Invoke(response);
            Completed = true;
        }

        private void catchExecute(Exception ex)
        {
            rejectFunc?.Invoke(ex);
            Completed = true;
            Failed = true;
        }
    }

    public class Promise : Promise<dynamic>
    {
        /// <summary>
        /// Dynamic Promise Constructor
        /// </summary>
        /// <example>
        /// new Promise(...) -- Doesn't have a type specifier
        /// </example>
        /// <inheritDoc/>
        public Promise(Func<dynamic> executor) : base(executor)
        {
        }

        /// <summary>
        /// Dynamic Executor Promise Constructor
        /// </summary>
        /// <example>
        /// new Promise(...) -- Doesn't have a type specifier
        /// </example>
        /// <inheritDoc/>
        public Promise(Action<Action<dynamic>, Action<string>> executor) : base(executor)
        {
        }
    }
}
