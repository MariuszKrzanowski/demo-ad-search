//-----------------------------------------------------------------------
// <copyright file="AllFoundGroups.cs" company="Mr Matrix Mariusz Krzanowski">
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


namespace MrMatrixNet.DemoAdGroupSearch.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class AllFoundGroups
    {
        ConcurrentQueue<ADGroupItem> _resolvedItems;

        public AllFoundGroups()
        {
            _resolvedItems = new ConcurrentQueue<ADGroupItem>();
        }
 
        public void Add(ADGroupItem resolvedGroup)
        {
            _resolvedItems.Enqueue(resolvedGroup);
        }

        public int Count
        {
            get
            {
                return _resolvedItems.Count;
            }
        }

        public IEnumerable<ADGroupItem> TakeAll()
        {
            return _resolvedItems;
        }

        public bool ContainsName(string groupName)
        {
            return _resolvedItems.Any(group=>string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
