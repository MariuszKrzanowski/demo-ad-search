//-----------------------------------------------------------------------
// <copyright file="QueryResultCoordinator.cs" company="Mr Matrix Mariusz Krzanowski">
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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using MrMatrixNet.DemoAdGroupSearch.Core;

    public sealed class QueryResultCoordinator : IDisposable
    {
        private ConcurrentDictionary<string, string> _distingushedName;
        private AllFoundGroups _allFoundGroups;
        private List<QueryEngine> _queryEngines;
        private int _handledPendingDistinguishedNamesCount;
        private int _pendingDistinguishedNamesCount;
        private ManualResetEvent _allDataRetrieved;

        public QueryResultCoordinator(IADFilterFactory adFilterFactory)
        {
            _distingushedName = new ConcurrentDictionary<string, string>();
            _allFoundGroups = new AllFoundGroups();
            _queryEngines=adFilterFactory
                .TakeFilters(this.ResolvedGroup)
                .Select(filter => new QueryEngine(this.HandledPendingItem, filter))
                .ToList();
            _allDataRetrieved = new ManualResetEvent(false);
        }

        private void HandledPendingItem(int count)
        {
            Interlocked.Add(ref _handledPendingDistinguishedNamesCount, count);
            RunStopTestCondition();
        }

        private void RunStopTestCondition()
        {
            if (Interlocked.CompareExchange(ref _handledPendingDistinguishedNamesCount, _pendingDistinguishedNamesCount, _pendingDistinguishedNamesCount) == _pendingDistinguishedNamesCount)
            {
                _allDataRetrieved.Set();
            }
        }

        private void ResolvedGroup(ADGroupItem resolvedGroupItem)
        {
            if(_distingushedName.TryAdd(resolvedGroupItem.DistinguishedName,resolvedGroupItem.DistinguishedName))
            {
                Interlocked.Add(ref _pendingDistinguishedNamesCount, _queryEngines.Count);
                _queryEngines.ForEach(engine => engine.Enque(resolvedGroupItem.DistinguishedName));
                _allFoundGroups.Add(resolvedGroupItem);
                RunStopTestCondition();
            }
        }

        public AllFoundGroups FindAllGroups(string userDistinguishedName)
        {
            Interlocked.Add(ref _pendingDistinguishedNamesCount, _queryEngines.Count);
            _queryEngines.ForEach(engine => engine.Enque(userDistinguishedName));
            RunStopTestCondition();
            _allDataRetrieved.WaitOne();
            _queryEngines.ForEach(engine => engine.Stop());
            return _allFoundGroups;
        }

        
        private bool _disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _allDataRetrieved.Dispose();
                }
                _disposedValue = true;
            }
        }

        ~QueryResultCoordinator()
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