using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using EdiHelper.Utilities;

namespace EdiHelper
{
    public class EdiDocumentBuilder
    {
        private readonly IEdiObjectReader _reader;

        public EdiDocumentBuilder(IEdiObjectReader reader)
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

        private EdiBaseSegment[] ReadChildren(XmlNodeList children)
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
                        //baseSegments[i] = ediSegmentGroup;
                        break;
                    case "segment":
                        // create segment
                        var ediSegment = ReadSegment(child);
                        if (ediSegment == null) continue;
                        foreach (var segment in ediSegment)
                        {
                            baseSegments.Add(pos, segment);
                            pos++;
                        }
                        //baseSegments[i] = ediSegment;
                        break;
                }
            }

            return baseSegments.Select(s => s.Value).ToArray(); ;
        }

        private EdiSegment[] ReadSegment(XmlNode segment)
        {
            var rows = segment.GetChildNodes("rows")?.SelectMany(rn => rn.GetChildNodes("row")).ToList();

            if (rows == null) return null;

            var queryableAttributes = segment.Attributes?.Cast<XmlAttribute>().AsQueryable()
                                      ?? new EnumerableQuery<XmlAttribute>(new List<XmlAttribute>());

            var segmentName = queryableAttributes.FirstOrDefault(a => a.Name == "tag")?.Value;

            // get node values from object reader and iterate results
            var nodeValues = _reader.Get(segmentName)?.ToArray() ?? new List<ICollection<KeyValuePair<string, string>>>() {new List<KeyValuePair<string, string>>()}.ToArray();
            var nvCount = nodeValues.Length;
            var rowCount = rows.Count();
            var segments = new EdiSegment[nvCount];

            for (var nv = 0; nv < nvCount; nv++)
            {
                var nvCollection = nodeValues[nv];

                // create new EdiSegment
                var ediSegment = new EdiSegment(segmentName, rowCount, new EdiUnaConfiguration());

                for (int r = 0; r < rowCount; r++)
                {
                    var cols = rows[r].GetChildNodes("col").ToList();
                    var colCount = cols.Count;

                    var colList = new List<string>();

                    for (int c = 0; c < colCount; c++)
                    {
                        var placeholder = cols[c].Attributes?.Cast<XmlAttribute>().AsQueryable()
                            .FirstOrDefault(ca => ca.Name == "placeholder")?.Value;

                        string nodeValue = null;

                        if (!string.IsNullOrEmpty(placeholder))
                        {
                            nodeValue = nvCollection.FirstOrDefault(v => v.Key == placeholder).Value;
                        }

                        // get value from placeholder or set default value
                        nodeValue = nodeValue ?? cols[c].FirstChild.Value;

                        Trace.WriteLine(nodeValue);
                        colList.Add(nodeValue);
                    }

                    if (colList.Count > 0)
                        ediSegment.Add(colList.ToArray());
                }

                segments[nv] = ediSegment;
            }

            return segments;
        }

        private EdiSegmentGroup ReadGroup(XmlNode node)
        {
            var attributes = node.Attributes?.Cast<XmlAttribute>().AsQueryable()
                ?? new EnumerableQuery<XmlAttribute>(new List<XmlAttribute>());

            var groupString = attributes.FirstOrDefault(a => a.Name == "id")?.Value;
            if (!int.TryParse(groupString, out var groupInt)) throw new Exception($"Invalid group id attribute at {node.Name}");
            
            // get children (recursive)
            var children = ReadChildren(node.ChildNodes);

            // create group entity
            var ediGroup = new EdiSegmentGroup(groupInt, children);

            return ediGroup;
        }
    }
}
