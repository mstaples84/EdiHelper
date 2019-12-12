using System.Text;

namespace EdiHelper
{
    /// <summary>
    /// Boxes Segments or other Segment Groups
    /// </summary>
    public class EdiSegmentGroup : EdiBaseSegment
    {
        private readonly EdiBaseSegment[] _baseSegments;
        private readonly int _groupId;

        public EdiSegmentGroup(int groupId, EdiBaseSegment[] segments)
        {
            _baseSegments = segments;
            _groupId = groupId;
        }

        public EdiBaseSegment this[int index] => _baseSegments[index];

        /// <summary>
        /// Returns a EDI formatted String of the group represented by the object.
        /// </summary>
        /// <returns>EDI formatted string</returns>
        public override string ToString()
        {
            // todo check if edi requires the group id somewhere here

            var sb = new StringBuilder();
            foreach (var ediSegment in _baseSegments)
            {
                sb.Append(ediSegment.ToString());
            }

            return sb.ToString();
        }
    }
}
