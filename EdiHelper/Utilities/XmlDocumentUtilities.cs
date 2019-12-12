using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EdiHelper.Utilities
{
    public static class XmlDocumentUtilities
    {
        public static IEnumerable<XmlNode> GetChildNodes(this XmlNode parent, string child)
        {
            return parent.SelectNodes(child)?.Cast<XmlNode>();
        }
    }
}
