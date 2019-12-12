using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdiHelper
{
    public class EdiDocument
    {
        /// <summary>
        /// Empty constructor creates an empty segments array
        /// </summary>
        public EdiDocument()
        {
            Segments = new EdiBaseSegment[0];
        }

        /// <summary>
        /// Instantiates the Segments Array with EdiSegment values
        /// </summary>
        /// <param name="segments"></param>
        public EdiDocument(EdiBaseSegment[] segments)
        {
            Segments = segments;
        }

        /// <summary>
        /// The EdiSegments in the document
        /// </summary>
        public EdiBaseSegment[] Segments { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var ediSegment in Segments)
            {
                sb.Append(ediSegment);
            }

            return sb.ToString();
        }
    }
}
