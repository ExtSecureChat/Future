using ExtSecureChat.Future.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExtSecureChat.Future
{
    /// <summary>
    /// Promise class with generic type
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    public class Promise<T>
    {
        public bool Completed { get; private set; }
        public bool Failed { get; private set; }
        public bool Cancelled { get; private set; }

        public T Result;
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
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute</param>
        public Promise(Func<T> executor)
        {
            InitializePromise(executor);
        }

        private void InitializePromise(Func<T> executor)
        {
            executorFunc = executor;

            // Create cancellation token and source
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executorTask = new Task<T>(() =>
            {
                try
                {
                    return executorExecute();
                }
                catch (Exception ex)
                {
                    // Set the global Exception variable and execute the reject function
                    Exception = ex;
                    rejectTask?.Start();
                    // Return a default value of T (Probably not neccesarily needed)
                    return default(T);
                }
            }, cancellationTokenSource.Token);

            executorTask.Start();
        }

        #region --- Executor Promise Methods ---

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own)
        /// </summary>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action<dynamic>, Action<Exception>> executor)
        {
            dynamic resolved = null;
            Exception exception = null;

            InitializePromise(() =>
            {
                // Execute these functions when they are called in the executor promise definition
                executor(
                    (resolve) =>
                    {
                        resolved = resolve;
                    },
                    (reject) =>
                    {
                        exception = new PromiseRejectException(reject.Message);
                    }
                );

                if (exception != null)
                {
                    throw exception;
                }

                return resolved;
            });
        }

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own) -- only reject
        /// </summary>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action<Exception>> executor)
        {
            dynamic resolved = null;
            Exception exception = null;

            InitializePromise(() =>
            {
                // Execute these functions when they are called in the executor promise definition
                executor(
                    (reject) =>
                    {
                        exception = new PromiseRejectException(reject.Message);
                    }
                );

                if (exception != null)
                {
                    throw exception;
                }

                return resolved;
            });
        }

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own) -- only resolve
        /// </summary>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action<dynamic>> executor)
        {
            dynamic resolved = null;

            InitializePromise(() =>
            {
                // Execute these functions when they are called in the executor promise definition
                executor(
                    (resolve) =>
                    {
                        resolved = resolve;
                    }
                );

                return resolved;
            });
        }

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own) -- without resolve result type
        /// </summary>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action, Action<Exception>> executor)
        {
            dynamic resolved = null;
            Exception exception = null;

            InitializePromise(() =>
            {
                // Execute these functions when they are called in the executor promise definition
                executor(
                    () => {
                        
                    },
                    (reject) =>
                    {
                        exception = new PromiseRejectException(reject.Message);
                    }
                );

                if (exception != null)
                {
                    throw exception;
                }

                return resolved;
            });
        }

        /// <summary>
        /// Generic Executor Promise Constructor (Resolve and reject on your own) -- only resolve without resolve result type
        /// </summary>
        /// <see cref="https://developer.mozilla.org/de/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
        /// <param name="executor">Function to execute with resolve and reject as arguments</param>
        public Promise(Action<Action> executor)
        {
            dynamic resolved = null;

            InitializePromise(() =>
            {
                // Execute these functions when they are called in the executor promise definition
                executor(
                    () => {}
                );

                return resolved;
            });
        }

        #endregion

        /// <summary>
        /// Action to execute after the promise successfully resolved
        /// </summary>
        /// <param name="resolve">Action to execute</param>
        /// <returns>The current promise</returns>
        public Promise<T> Then(Action<T> resolve)
        {
            resolveFunc = resolve;

            resolveTask = executorTask.ContinueWith(t =>
            {
                resolveExecute(t.Result);
            }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);

            return this;
        }

        /// <summary>
        /// Action to execute after the promise faulted/failed or threw an exception
        /// </summary>
        /// <param name="reject">Action to execute</param>
        /// <returns>The current promise</returns>
        public Promise<T> Catch(Action<Exception> reject)
        {
            rejectFunc = reject;

            rejectTask = new Task(() =>
            {
                rejectExecute(Exception.InnerException);
            });

            return this;
        }

        /// <summary>
        /// Waits for the promise to finish
        /// </summary>
        public void Wait()
        {
            executorTask?.Wait();
            resolveTask?.Wait();
            rejectTask?.Wait();
        }

        /// <summary>
        /// Cancels the promise
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            Completed = true;
            Cancelled = true;
        }

        private T executorExecute()
        {
            cancellationToken.ThrowIfCancellationRequested();

            var t = Task.Factory.StartNew(() =>
            {
                return executorFunc.Invoke();
            });

            // Check for cancellation whilst the promise is not finished
            while (!t.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            // Complete the promise if there is no resolve function
            if (resolveFunc == null)
            {
                Completed = true;
            }

            Result = t.Result;
            return t.Result;
        }

        private void resolveExecute(T response)
        {
            resolveFunc?.Invoke(response);
            Completed = true;
        }

        private void rejectExecute(Exception ex)
        {
            rejectFunc?.Invoke(ex);
            Completed = true;
            Failed = true;
        }

        #region --- Static Methods ---

        /// <summary>
        /// Waits for all provided promises to finish
        /// </summary>
        /// <param name="promises">Promises to wait for</param>
        /// <returns>A new promise</returns>
        public static Promise<dynamic> All(params Promise<dynamic>[] promises)
        {
            return new Promise((resolve, reject) =>
            {
                // This is maybe the most easy and efficient solution
                foreach (var promise in promises)
                {
                    promise.Wait();
                }
                resolve();
            });
        }

        /// <summary>
        /// Waits for one of the promises to finish and returns it
        /// </summary>
        /// <param name="promises"></param>
        /// <returns>First promise that finished</returns>
        public static Promise<dynamic> Race(params Promise<dynamic>[] promises)
        {
            List<Task> tasks = new List<Task>();
            Promise<dynamic> completed = null;
            foreach (var promise in promises)
            {
                tasks.Add(new Task(() =>
                {
                    promise.Wait();
                }));
            }

            // Wait for a promise to complete
            foreach (var task in tasks)
            {
                task.Start();
            }
            int i = Task.WaitAny(tasks.ToArray());
            completed = promises[i];
            return completed;
        }

        #endregion
    }

    /// <summary>
    /// Promise Class without generic type
    /// </summary>
    public class Promise : Promise<dynamic>
    {
        /// <inheritDoc/>
        public Promise(Func<dynamic> executor) : base(executor)
        {
        }

        /// <inheritDoc/>
        public Promise(Action<Action<dynamic>, Action<Exception>> executor) : base(executor)
        {
        }

        /// <inheritDoc/>
        public Promise(Action<Action<dynamic>> executor) : base(executor)
        {
        }

        /// <inheritDoc/>
        public Promise(Action<Action<Exception>> executor) : base(executor)
        {
        }

        /// <inheritDoc/>
        public Promise(Action<Action, Action<Exception>> executor) : base(executor)
        {
        }

        /// <inheritDoc/>
        public Promise(Action<Action> executor) : base(executor)
        {
        }

        #region --- Static Methods ---

        /// <inheritDoc/>
        public new static Promise<dynamic> All(params Promise<dynamic>[] promises)
        {
            return Promise<dynamic>.All(promises);
        }

        /// <inheritDoc/>
        public new static Promise<dynamic> Race(params Promise<dynamic>[] promises)
        {
            return Promise<dynamic>.Race(promises);
        }

        #endregion
    }
}
