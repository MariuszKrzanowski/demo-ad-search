//-----------------------------------------------------------------------
// <copyright file="ADFilterSample.cs" company="Mr Matrix Mariusz Krzanowski">
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
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Text;
    using MrMatrixNet.DemoAdGroupSearch.Core;

    public class ADFilterSample : IADFilter
    {
        string _directoryPath;
        GroupItemResolved _resolvedGroup;

        private const string DistingushedName = "dn";
        private const string Name = "name";
        private const string Member = "member";

        public ADFilterSample(string directoryPath)
        {
            _directoryPath = directoryPath;

        }

        public void RegisterResolvedGroupHandler(GroupItemResolved resolvedGroup)
        {
            _resolvedGroup = resolvedGroup;
        }

        public void Resolve(List<string> itemsToDo)
        {
            if (itemsToDo.Count == 0)
            {
                return;
            }



            using (DirectoryEntry de = new DirectoryEntry(_directoryPath))
            {
                using (DirectorySearcher ds
                    = new DirectorySearcher(de,
                    BuilFilter(itemsToDo), new string[] {
                        DistingushedName,
                        Name
                    }))
                {
                    ds.Asynchronous = true;
                    ds.PageSize = 100;
                    ds.SearchScope = SearchScope.Subtree;

                    var result = ds.FindAll();
                    if (result == null)
                    {
                        return;
                    }

                    foreach (SearchResult foundItem in result)
                    {
                        string dn = (string)foundItem.Properties[DistingushedName][0];
                        string name = (string)foundItem.Properties[Name][0];
                        _resolvedGroup(new ADGroupItem(dn, name));
                    }
                }


            }
        }

        private string BuilFilter(List<string> itemsToDo)
        {
            if (0 == itemsToDo.Count)
            {
                throw new ArgumentException(nameof(itemsToDo));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("(&(objectClass=group)");

            if (itemsToDo.Count == 1)
            {
                return string.Concat("(", Member, "=", itemsToDo[0], ")");
            }
            else
            {
                sb.Append("(|");
                foreach (string item in itemsToDo)
                {
                    sb.Append("(");
                    sb.Append(Member);
                    sb.Append("=");
                    sb.Append(item);
                    sb.Append(")");

                }
                sb.Append(")");
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}
