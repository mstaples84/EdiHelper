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
        public EdiDocument Create(XmlDocument xmlDoc) // todo add object mapper using DI
        {
            if (xmlDoc.DocumentElement == null) return null;

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
            var baseSegments = new EdiBaseSegment[childCount];

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
                        baseSegments[i] = ediSegmentGroup;
                        break;
                    case "segment":
                        // create segment
                        var ediSegment = ReadSegment(child);
                        if (ediSegment == null) continue;
                        baseSegments[i] = ediSegment;
                        break;
                }
            }

            return baseSegments;
        }

        private EdiSegment ReadSegment(XmlNode segment)
        {
            var rows = segment.GetChildNodes("rows")?.SelectMany(rn => rn.GetChildNodes("row")).ToList();

            if (rows == null) return null;

            var queryableAttributes = segment.Attributes?.Cast<XmlAttribute>().AsQueryable()
                                      ?? new EnumerableQuery<XmlAttribute>(new List<XmlAttribute>());

            var segmentName = queryableAttributes.FirstOrDefault(a => a.Name == "tag")?.Value;

            // todo get node values from object reader and iterate results

            var rowCount = rows.Count();

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

                    // todo get value from placeholder or set default value

                    var value = cols[c].FirstChild.Value;
                    Trace.WriteLine(value);
                    colList.Add(value);
                }

                if (colList.Count > 0)
                    ediSegment.Add(colList.ToArray());
            }

            return ediSegment;
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
