//-----------------------------------------------------------------------
// <copyright file="QueryEngineTest.cs" company="Mr Matrix Mariusz Krzanowski">
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

namespace MrMatrixNet.DemoAdGroupSearch.Tests
{
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MrMatrixNet.DemoAdGroupSearch.Core;
    using MrMatrixNet.DemoAdGroupSearch.Engine;
    using MrMatrixNet.DemoAdGroupSearch.Mocks;

    [TestClass]
    public class QueryEngineTest
    {
        [TestMethod]
        public void StartStop()
        {
            PendingItemsHandled hpi = (count) =>
            {
            };
            GroupItemResolved rg = (resolvedGroupItem) =>
            {
            };
            var adf = new ADFilterPong();
            adf.RegisterResolvedGroupHandler(rg);
            var queryEngine = new QueryEngine(hpi, new ADFilterPong());
            queryEngine.Stop();
        }

        [TestMethod]
        public void WhenEnquedItemsFilterMethodsAreCalled()
        {
            int handledItemsCount = 0;
            int resolvedItemsCount = 0;

            PendingItemsHandled hpi = (count) =>
            {
                Interlocked.Increment(ref handledItemsCount);
            };
            GroupItemResolved rg = (resolvedGroupItem) =>
            {
                Interlocked.Increment(ref resolvedItemsCount);
            };
            var adf = new ADFilterPong();
            adf.RegisterResolvedGroupHandler(rg);
            var queryEngine = new QueryEngine(hpi, new ADFilterPong());
            queryEngine.Enque("a");
            System.Threading.Thread.Sleep(0);
            queryEngine.Enque("b");
            while (Interlocked.CompareExchange(ref handledItemsCount, 2, 2) == 2)
            {
                System.Threading.Thread.Sleep(0);
            }

            queryEngine.Stop();
        }

        [TestMethod]
        public void WhenEnquedItemsFilterDeliveredTwiceMethodsAreCalled()
        {
            int handledItemsCount = 0;
            int resolvedItemsCount = 0;

            PendingItemsHandled hpi = (count) =>
            {
                Interlocked.Increment(ref handledItemsCount);
            };
            GroupItemResolved rg = (resolvedGroupItem) =>
            {
                Interlocked.Increment(ref resolvedItemsCount);
            };
            var adf = new ADFilterPong();
            adf.RegisterResolvedGroupHandler(rg);
            var queryEngine = new QueryEngine(hpi, new ADFilterPong());
            queryEngine.Enque("a");
            queryEngine.Enque("b");
            while (Interlocked.CompareExchange(ref handledItemsCount, 2, 2) == 2)
            {
                System.Threading.Thread.Sleep(0);
            }

            queryEngine.Enque("c");
            while (Interlocked.CompareExchange(ref handledItemsCount, 3, 3) == 3)
            {
                System.Threading.Thread.Sleep(0);
            }

            queryEngine.Stop();
        }
    }
}
