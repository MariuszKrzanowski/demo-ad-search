//-----------------------------------------------------------------------
// <copyright file="EmulatedADFilterFactoryMock.cs" company="Mr Matrix Mariusz Krzanowski">
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

namespace MrMatrixNet.DemoAdGroupSearch.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using MrMatrixNet.DemoAdGroupSearch.Core;

    public class EmulatedADFilterFactoryMock : IADFilterFactory
    {
        private static readonly ADZone[] AllZones = new ADZone[]
               {
                        new ADZone(
                            new ADGroup("GA1", "GroupA1", "U1", "GC2", "GC4"),
                            new ADGroup("GA2", "GroupA2", "GB1", "GB2", "GA3", "GC1"),
                            new ADGroup("GA3", "GroupA3", "GB1", "GC1"),
                            new ADGroup("GA4", "GroupA4"),
                            new ADGroup("GA5", "GroupA5", "GA1")),
                        new ADZone(
                            new ADGroup("GB1", "GroupB1", "U1"),
                            new ADGroup("GB2", "GroupB2"),
                            new ADGroup("GB3", "GroupB3", "GA1"),
                            new ADGroup("GB4", "GroupB4", "GA3")),
                        new ADZone(
                            new ADGroup("GC1", "GroupC1", "GB4"),
                            new ADGroup("GC2", "GroupC2"),
                            new ADGroup("GC3", "GroupC3"),
                            new ADGroup("GC4", "GroupC4", "GA1"),
                            new ADGroup("GC5", "GroupC5", "UD"),
                            new ADGroup("GC6", "GroupC6", "UD"))
               };

        public IEnumerable<IADFilter> TakeFilters(GroupItemResolved resolvedGroup)
        {
            return AllZones.Select(zone => FilterBuilder(resolvedGroup, zone));
        }

        private static ADFilter FilterBuilder(GroupItemResolved resolvedGroup, ADZone zone)
        {
            var filter = new ADFilter(zone);
            filter.RegisterResolvedGroupHandler(resolvedGroup);
            return filter;
        }

        private class ADGroup
        {
            private string _dn;
            private string _name;
            private HashSet<string> _members;

            public ADGroup(string dn, string name, params string[] members)
            {
                _dn = dn;
                _name = name;
                if (members != null)
                {
                    _members = new HashSet<string>(members, StringComparer.OrdinalIgnoreCase);
                    return;
                }

                _members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public string DistinguishedName
            {
                get
                {
                    return _dn;
                }
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public bool Contains(string dn)
            {
                return _members.Contains(dn);
            }
        }

        private class ADZone
        {
            private List<ADGroup> _groups;
            private Random _networkLatencyEmulator = new Random();

            public ADZone(params ADGroup[] groups)
            {
                _groups = new List<ADGroup>(groups);
            }

            internal IEnumerable<ADGroupItem> FindGroups(string dn)
            {
                /// Emulation of network latency
                Thread.Sleep(_networkLatencyEmulator.Next(4, 12));
                return _groups
                    .Where(group => group.Contains(dn))
                    .Select(group => new ADGroupItem(group.DistinguishedName, group.Name));
            }
        }

        private class ADFilter : IADFilter
        {
            private GroupItemResolved _resolvedGroup;
            private ADZone _zone;

            public ADFilter(ADZone zone)
            {
                _zone = zone;
            }

            public void Resolve(List<string> itemsToDo)
            {
                itemsToDo.ForEach(
                    item => _zone
                    .FindGroups(item)
                    .ToList()
                    .ForEach(resolvedGroupItem => _resolvedGroup?.Invoke(resolvedGroupItem)));
            }

            public void RegisterResolvedGroupHandler(GroupItemResolved resolvedGroup)
            {
                _resolvedGroup = resolvedGroup;
            }
        }
    }
}
