//-----------------------------------------------------------------------
// <copyright file="ADFilterSampleTest.cs" company="Mr Matrix Mariusz Krzanowski">
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
    using System;
    using System.Collections.Generic;
    using Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ADFilterSampleTest
    {
        /// <summary>
        /// When more than one distinguished name is provided then OR  ( | ) operator
        /// should be used to prepare filter.
        /// </summary>
        [TestMethod]
        public void When_TwoDNsAreGiven_Then_PrepareValidADQueryFilterForGroupsWithOr()
        {
            List<string> dnToFind = new List<string>()
            {
                "CN=g1,OU=Groups,DC=X,DC=sample,DC=com",
                "CN=g2,OU=Groups,DC=X,DC=sample,DC=com",
            };

            Assert.AreEqual(
                "(&(objectClass=group)(|(member=CN=g1,OU=Groups,DC=X,DC=sample,DC=com)(member=CN=g2,OU=Groups,DC=X,DC=sample,DC=com)))",
                ADFilterSample.BuildGroupFilter(dnToFind));
        }

        /// <summary>
        /// When only one distinguished name is provided then DN should 
        /// be inserted inside parenthesis only. 
        /// </summary>
        [TestMethod]
        public void When_SingleDNsAreGiven_Then_PrepareValidADQueryFilterWithSingleMemberWithoutOr()
        {
            List<string> dnToFind = new List<string>()
            {
                "CN=g1,OU=Groups,DC=X,DC=sample,DC=com"
            };

            Assert.AreEqual(
                "(&(objectClass=group)(member=CN=g1,OU=Groups,DC=X,DC=sample,DC=com))",
                ADFilterSample.BuildGroupFilter(dnToFind));
        }

        /// <summary>
        /// Filter for empty collection should not be created.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void When_EmptyDNsCollectionIsGiven_Then_ThrowException()
        {
            List<string> dnToFind = new List<string>();

            ADFilterSample.BuildGroupFilter(dnToFind);
        }
    }
}
