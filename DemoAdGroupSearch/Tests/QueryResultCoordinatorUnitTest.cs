//-----------------------------------------------------------------------
// <copyright file="QueryResultCoordinatorUnitTest.cs" company="Mr Matrix Mariusz Krzanowski">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MrMatrixNet.DemoAdGroupSearch.Core;
    using MrMatrixNet.DemoAdGroupSearch.Engine;
    using MrMatrixNet.DemoAdGroupSearch.Mocks;

    [TestClass]
    public class QueryResultCoordinatorUnitTest
    {
        [TestMethod]
        public void SampleOfUsageConcurentDictionaryInstedOfHashSetAsParallel()
        {
            ConcurrentDictionary<string, string> dict = new ConcurrentDictionary<string, string>();
            int counter = 0;

            (new int[] { 1, 2, 3 }).ToList().AsParallel().ForAll(i =>
            {
                if (dict.TryAdd("1", "1"))
                {
                    Interlocked.Increment(ref counter);
                }
            });
     
            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public void WhenNoZonesStopProcessing()
        {
            var qrc = new QueryResultCoordinator(new EmptyADFilterFactoryMock());
            var allGroups = qrc.FindAllGroups("U1");
            Assert.AreEqual(0, allGroups.Count);
        }

        [TestMethod]
        public void WhenUserHaveNoGroupReturnEmptySet()
        {
            var qrc = new QueryResultCoordinator(new EmulatedADFilterFactoryMock());
            var allGroups = qrc.FindAllGroups("UX");
            Assert.AreEqual(0, allGroups.Count);
        }

        [TestMethod]
        public void WhenUserHaveDirectGroupOnlyReturnThem()
        {
            var qrc = new QueryResultCoordinator(new EmulatedADFilterFactoryMock());
            var allGroups = qrc.FindAllGroups("UD");
            Assert.AreEqual(2, allGroups.Count);
        }

        [TestMethod]
        public void WhenUserHaveIndrectGroupReturnThem()
        {
            var qrc = new QueryResultCoordinator(new EmulatedADFilterFactoryMock());
            var allGroups = qrc.FindAllGroups("U1");
            Assert.AreEqual(9, allGroups.Count);
        }

        private class EmptyADFilterFactoryMock : IADFilterFactory
        {
            public IEnumerable<IADFilter> TakeFilters(GroupItemResolved resolvedGroup)
            {
                return new IADFilter[0];
            }
        }
    }
}
