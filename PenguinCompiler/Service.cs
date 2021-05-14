using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public abstract class Service<TRequest> : IDisposable
    {
        private readonly ConcurrentQueue<TRequest> _requestQueue;
        private int _workingCount;

        public int MaxRequest { get; }
        public int MaxTask { get; }
        protected CancellationTokenSource CancelSource { get; }

        public Service(int maxRequest, int maxTask)
        {
            MaxRequest = maxRequest;
            MaxTask = maxTask;
            _requestQueue = new ConcurrentQueue<TRequest>();
            CancelSource = new CancellationTokenSource();
            _workingCount = 0;
        }

        public async Task StartAsync()
        {
            Start();
            StartWorkingTaskAsync();
            var cancelToken = CancelSource.Token;
            while (true)
            {
                try
                {
                    cancelToken.ThrowIfCancellationRequested();
                    var request = await GetRequestAsync();
                    if (!IsValidRequest(request))
                    {
                        continue;
                    }
                    AddRequest(request);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void Stop()
        {
            while (!CancelSource.IsCancellationRequested)
            {
                CancelSource.Cancel(true);
            }
            Dispose();
            CancelSource.Dispose();
        }

        public virtual void Dispose() { GC.SuppressFinalize(this); }

        private async void StartWorkingTaskAsync()
        {
            var cancelToken = CancelSource.Token;
            while (true)
            {
                try
                {
                    await Task.Delay(10, cancelToken);
                    while (_requestQueue.IsEmpty) //等待下一个请求
                    {
                        await Task.Delay(50, cancelToken);
                    }

                    while (_workingCount >= MaxTask) //等待工作结束
                    {
                        await Task.Delay(50, cancelToken);
                    }

                    TRequest request;
                    while (!_requestQueue.TryDequeue(out request)) //尝试取出请求
                    {
                        await Task.Delay(5, cancelToken);
                    }

                    Interlocked.Increment(ref _workingCount);
                    var task = RunTaskAsync(request);
                    _ = task.ContinueWith(_ => Interlocked.Decrement(ref _workingCount), cancelToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void AddRequest(TRequest request)
        {
            if (_requestQueue.Count >= MaxRequest)
            {
                HandleDiscardedRequest(request);
            }
            else
            {
                _requestQueue.Enqueue(request);
            }
        }

        protected abstract void Start();

        protected virtual async Task<TRequest> GetRequestAsync()
        {
            await Task.Delay(-1, CancelSource.Token);
            return default;
        }

        protected virtual bool IsValidRequest(TRequest request) { return request != null; }

        protected virtual void HandleDiscardedRequest(TRequest request) { }

        protected abstract Task RunTaskAsync(TRequest request);
    }
}