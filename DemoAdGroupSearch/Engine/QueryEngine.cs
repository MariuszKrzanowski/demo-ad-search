//-----------------------------------------------------------------------
// <copyright file="QueryEngine.cs" company="Mr Matrix Mariusz Krzanowski">
//     (c) 2017 Mr Matrix Mariusz Krzanowski 
// </copyright>
// <author>Mariusz Krzanowski</author>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
//-----------------------------------------------------------------------

namespace MrMatrixNet.DemoAdGroupSearch.Engine
{
    using Core;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class QueryEngine : IDisposable
    {
        private ConcurrentQueue<string> _pendingQueue;
        private PendingItemsHandled _handledPendingItems;
        private IADFilter _adFilter;
        private int _handledItemsCounter;
        private int _enqueued;
        private int _dequeued;
        private const int BatchSize = 10;

        private Thread _worker;

        CancellationTokenSource _stopToken;
        private ManualResetEventSlim _canConsume;
        private SemaphoreSlim _semaphoreSlim;

        public QueryEngine(PendingItemsHandled handledPendingItems,
            IADFilter adFilter
            )
        {
            _semaphoreSlim = new SemaphoreSlim(1);
            _stopToken = new CancellationTokenSource();
            _canConsume = new ManualResetEventSlim(false);
            _handledPendingItems = handledPendingItems;
            _adFilter = adFilter;
            _pendingQueue = new ConcurrentQueue<string>();

            _worker = new Thread(this.Run);
            _worker.Start();

        }

        public void Stop()
        {
            _stopToken.Cancel();
            _worker.Join();
        }

        private void Run()
        {
            while (true)
            {
                try
                {
                    _canConsume.Wait(_stopToken.Token);
                    HandlePendingItems();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        public void Enque(string dn)
        {
            Interlocked.Increment(ref _enqueued);
            _pendingQueue.Enqueue(dn);
            VerifyIfWorkerHaveSomethingToDo();
        }

        private void VerifyIfWorkerHaveSomethingToDo()
        {
            try
            {
                _semaphoreSlim.Wait();
                if (_enqueued == _dequeued)
                {
                    _canConsume.Reset();
                }
                else
                {
                    _canConsume.Set();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void HandlePendingItems()
        {
            List<string> pendingItems = new List<string>();
            for (int itemsToDoCounter = 0; itemsToDoCounter < BatchSize; itemsToDoCounter++)
            {
                string itemToDo;
                if (_pendingQueue.TryDequeue(out itemToDo))
                {
                    pendingItems.Add(itemToDo);
                }
                else
                {
                    break;
                }
            }

            Interlocked.Add(ref _dequeued, pendingItems.Count);
            if (pendingItems.Count > 0)
            {
                _adFilter.Resolve(pendingItems);
            }

            _handledItemsCounter += pendingItems.Count;
            if (pendingItems.Count == 0)
            {
                if (_handledItemsCounter != 0)
                {
                    _handledPendingItems(_handledItemsCounter);
                }

                _handledItemsCounter = 0;
                VerifyIfWorkerHaveSomethingToDo();
            }
        }

        private bool _disposedValue = false; 
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _stopToken.Dispose();
                    _semaphoreSlim.Dispose();
                    _canConsume.Dispose();
                }
                _disposedValue = true;
            }
        }

        ~QueryEngine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
