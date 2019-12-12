namespace EdiHelper
{
    public class EdiUnaConfiguration
    {
        public string UnaSingleDelimiter { get; set; }
        public string UnaCompositeDelimiter { get; set; }
        public string UnaSegmentDelimiter { get; set; }

        public EdiUnaConfiguration()
        {
            UnaSingleDelimiter = "+";
            UnaCompositeDelimiter = ":";
            UnaSegmentDelimiter = "'";
        }
    }
}
