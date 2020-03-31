using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using EdiHelper.Utilities;

namespace EdiHelper
{
    public class EdiDocumentBuilder2
    {
        private readonly EdiObjectReader2 _reader;

        public EdiDocumentBuilder2(EdiObjectReader2 reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Creates a EdiDocument from the object
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="o">Object with EdiAttribute tags to read values from</param>
        /// <returns></returns>
        public EdiDocument Create(XmlDocument xmlDoc, object o)
        {
            if (xmlDoc.DocumentElement == null) return null;
            if (o == null) return null;

            _reader.Read(o);

            var segmentsNodes = xmlDoc.GetElementsByTagName("segments");
            if (segmentsNodes.Count != 1) return null;

            // get child nodes
            var segmentsChildren = segmentsNodes[0].ChildNodes;

            var docSegments = ReadChildren(segmentsChildren);
            
            return new EdiDocument(docSegments);
        }

        private EdiBaseSegment[] ReadChildren(XmlNodeList children, ICollection<Tuple<string, string, string>> group = null)
        {
            var childCount = children.Count;
            var baseSegments = new SortedList<int, EdiBaseSegment>();
            var pos = 0;
            //var baseSegments = new EdiBaseSegment[childCount];

            for (int i = 0; i < childCount; i++)
            {
                var child = children[i];
                switch (child.Name)
                {
                    case "segmentgroup":
                        // create segment group
                        // segments and sub segments build themself recursively
                        var ediSegmentGroup = ReadGroup(child);
                        if (ediSegmentGroup == null) continue;
                        baseSegments.Add(pos, ediSegmentGroup);
                        pos++;
                        break;
                    case "segment":
                        // create segment
                        if (group == null) group = _reader.GetSegmentGroup(0)?.First();
                        var ediSegment = ReadSegment(child, group);
                        if (ediSegment == null) continue;
                        //foreach (var segment in ediSegment)
                        //{
                        baseSegments.Add(pos, ediSegment);
                        pos++;
                        //}
                        break;
                }
            }

            return baseSegments.Select(s => s.Value).ToArray();
        }

        private EdiSegment ReadSegment(XmlNode segment, ICollection<Tuple<string, string, string>> group)
        {
            var rows = segment.GetChildNodes("rows")?.SelectMany(rn => rn.GetChildNodes("row")).ToList();

            if (rows == null) return null;

            var queryableAttributes = segment.Attributes?.Cast<XmlAttribute>().AsQueryable()
                                      ?? new EnumerableQuery<XmlAttribute>(new List<XmlAttribute>());

            var segmentName = queryableAttributes.FirstOrDefault(a => a.Name == "tag")?.Value;
            
            // get node values from object reader and iterate results
            //var nodeValues = _reader.Get(segmentName)?.ToArray() ?? new List<ICollection<Tuple<string,string,int>>>() {new List<Tuple<string, string, int>>()}.ToArray();
            var oValues = group?.Where(g => g.Item1 == segmentName).ToList() ?? new List<Tuple<string, string, string>>();

            var rowCount = rows.Count();

            var ediSegment = new EdiSegment(segmentName, rowCount, new EdiUnaConfiguration());

            for (int r = 0; r < rowCount; r++) {
                var cols = rows[r].GetChildNodes("col").ToList();
                var colCount = cols.Count;

                var colList = new List<string>();

                for (int c = 0; c < colCount; c++) {
                    var placeholder = cols[c].Attributes?.Cast<XmlAttribute>().AsQueryable()
                        .FirstOrDefault(ca => ca.Name == "placeholder")?.Value;

                    string nodeValue = null;

                    if (!string.IsNullOrEmpty(placeholder)) {
                        nodeValue = oValues.FirstOrDefault(nv => nv.Item2 == placeholder)?.Item3;
                        //nodeValue = nvCollection.FirstOrDefault(v => v.Item1 == placeholder && v.Item3 == groupId)?.Item2;

                    }

                    // get value from placeholder or set default value
                    nodeValue = nodeValue ?? cols[c].FirstChild?.Value;

                    if (string.IsNullOrEmpty(nodeValue)) continue;

                    Trace.WriteLine(nodeValue);
                    colList.Add(nodeValue);
                }

                if (colList.Count > 0)
                    ediSegment.Add(colList.ToArray());
            }

            return ediSegment;
            
            //var nvCount = nodeValues.Length;
            //var segments = new EdiSegment[nvCount];
            //var rowCount = rows.Count();

            //for (var nv = 0; nv < nvCount; nv++)
            //{
            //    var nvCollection = nodeValues[nv];

            //    // create new EdiSegment
            //    var ediSegment = new EdiSegment(segmentName, rowCount, new EdiUnaConfiguration());

            //    for (int r = 0; r < rowCount; r++)
            //    {
            //        var cols = rows[r].GetChildNodes("col").ToList();
            //        var colCount = cols.Count;

            //        var colList = new List<string>();

            //        for (int c = 0; c < colCount; c++)
            //        {
            //            var placeholder = cols[c].Attributes?.Cast<XmlAttribute>().AsQueryable()
            //                .FirstOrDefault(ca => ca.Name == "placeholder")?.Value;

            //            string nodeValue = null;

            //            if (!string.IsNullOrEmpty(placeholder))
            //            {
            //                nodeValue = nvCollection.FirstOrDefault(v => v.Item1 == placeholder && v.Item3 == groupId)?.Item2;
            //            }

            //            // get value from placeholder or set default value
            //            nodeValue = nodeValue ?? cols[c].FirstChild?.Value;

            //            if (string.IsNullOrEmpty(nodeValue)) continue;

            //            Trace.WriteLine(nodeValue);
            //            colList.Add(nodeValue);
            //        }

            //        if (colList.Count > 0)
            //            ediSegment.Add(colList.ToArray());
            //    }

            //    segments[nv] = ediSegment;
            //}

            //return segments;
        }

        private EdiSegmentGroup ReadGroup(XmlNode node)
        {
            var attributes = node.Attributes?.Cast<XmlAttribute>().AsQueryable()
                ?? new EnumerableQuery<XmlAttribute>(new List<XmlAttribute>());

            var groupString = attributes.FirstOrDefault(a => a.Name == "id")?.Value;
            if (!int.TryParse(groupString, out var groupInt)) throw new Exception($"Invalid group id attribute at {node.Name}");

            var segmentGroup = _reader.GetSegmentGroup(groupInt);
            
            EdiBaseSegment[] children = null;

            if (segmentGroup == null)
                children = ReadChildren(node.ChildNodes);
            
            else
            {
                foreach (var group in segmentGroup) {
                    // Childrens will be overwritten. Need to be fixed
                    // get children (recursive)
                    children = ReadChildren(node.ChildNodes, group);
                }
            }
            
            if (children == null) return null;

            // create group entity
            var ediGroup = new EdiSegmentGroup(groupInt, children);

            return ediGroup;
        }
    }
}
