using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExtSecureChat.Future
{
    public class Promise<T>
    {
        public bool Completed { get; private set; }
        public bool Failed { get; private set; }
        public bool Cancelled { get; private set; }

        public Exception Exception { get; private set; }

        private Task<T> task;
        private Task continueTask;
        private Task catchTask;
        private Task finallyTask;

        private Func<T> executeFunc;
        private Action<T> thenFunc;
        private Action<Exception> catchFunc;
        private Action finallyFunc;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        public Promise(Func<T> execute, Action<T> then = null, Action<Exception> except = null, Action final = null)
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            executeFunc = execute;
            thenFunc = then;
            catchFunc = except;
            finallyFunc = final;

            task = new Task<T>(() =>
            {
                try
                {
                    return TaskExecute();
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    catchTask.Start();
                    return default(T);
                }
                
            }, cancellationTokenSource.Token);

            continueTask = task.ContinueWith(t =>
            {
                thenExecute(t.Result);
            }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);

            catchTask = new Task(() =>
            {
                catchExecute(Exception);
            });

            finallyTask = task.ContinueWith(t =>
            {
                finallyExecute();
            });

            task.Start();
        }

        public void Wait()
        {
            task?.Wait();
            continueTask?.Wait();
            if (Failed)
            {
                catchTask?.Wait();
            }
            finallyTask?.Wait();
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            Cancelled = true;
        }

        private T TaskExecute()
        {
            cancellationToken.ThrowIfCancellationRequested();

            var t =  Task.Factory.StartNew(() =>
            {
                return executeFunc.Invoke();
            });

            while (!t.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            if (thenFunc == null || finallyFunc == null)
            {
                Completed = true;
            }

            return t.Result;
        }

        private void thenExecute(T response)
        {
            thenFunc?.Invoke(response);
            Completed = true;
        }

        private void catchExecute(Exception ex)
        {
            catchFunc?.Invoke(ex);
            Completed = true;
            Failed = true;
        }

        private void finallyExecute()
        {
            finallyFunc?.Invoke();
            Completed = true;
        }
    }
}
