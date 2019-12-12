using System.Text;

namespace EdiHelper
{
    /// <summary>
    /// Represents a EDI Segment an contains single and composite values.
    /// </summary>
    public class EdiSegment : EdiBaseSegment
    {
        /// <summary>
        /// Segment Values are saved as 2 dimensional jagged array.
        /// Position 0 is the single value, or first value of the composite type.
        /// Positions 1-n are the composite values.
        /// </summary>
        private readonly string[][] _segmentValues;

        // used by Add to allocate next free array position
        private int _lastIndex;
        private readonly EdiUnaConfiguration _unaConfiguration;

        /// <summary>
        /// Represents the three digit Tag defined in EDI.
        /// </summary>
        public string SegmentTag
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentTag">The Tag of this segment (FII, RFF, DOC, ...).</param>
        /// <param name="valuesLength">Number of Values this Tag has by definition.</param>
        /// <param name="unaConfiguration">UNA Configuration defined by EDI standard.</param>
        public EdiSegment(string segmentTag, 
            int valuesLength,
            EdiUnaConfiguration unaConfiguration)
        {
            SegmentTag = segmentTag;
            _segmentValues = new string[valuesLength][];
            _unaConfiguration = unaConfiguration;
        }

        /// <summary>
        /// Adds a single ([0]) or composite ([1-n]) value
        /// </summary>
        /// <param name="values">The values for the Segment</param>
        public void Add(string[] values)
        {
            _segmentValues[_lastIndex] = values;
            ++_lastIndex;
        }

        /// <summary>
        /// Returns the EDI formatted string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var segmentLength = _segmentValues.Length;
            var sb = new StringBuilder();

            sb.Append(SegmentTag);

            for (int i = 0; i < segmentLength; i++)
            {
                var segmentValue = _segmentValues[i];
                if (segmentValue == null) continue;

                var valuesLength = segmentValue.Length;

                sb.Append(_unaConfiguration.UnaSingleDelimiter); // append value delimiter

                for (int vIndex = 0; vIndex < valuesLength; vIndex++)
                {
                    if (vIndex > 0) sb.Append(_unaConfiguration.UnaCompositeDelimiter); // append composite delimiter
                    sb.Append(_segmentValues[i][vIndex]); // append value
                }
            }

            sb.AppendFormat("{0}{1}", _unaConfiguration.UnaSegmentDelimiter, "\r\n");

            return sb.ToString();
        }
    }
}
