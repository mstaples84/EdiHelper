using EdiHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiHelperTests
{
    [TestClass]
    public class EdiSegmentTests
    {
        [TestMethod]
        public void TestUnbMessage()
        {
            var segment = new EdiSegment("UNB", 11, new EdiUnaConfiguration());

            segment.Add(new[] { "UNOC", "3" });
            segment.Add(new[] { "4123...", "14" });
            segment.Add(new[] { "4260...", "14" });
            segment.Add(new[] { "070101", "1321" });
            segment.Add(new[] { "31" });
            segment.Add(new string[0]);
            segment.Add(new[] { "SGH-DUO" });
            segment.Add(new string[0]);
            segment.Add(new string[0]);
            segment.Add(new string[0]);
            segment.Add(new[] { "1" });

            var segmentString = segment.ToString();

            Assert.AreEqual("UNB+UNOC:3+4123...:14+4260...:14+070101:1321+31++SGH-DUO++++1\'\r\n", segmentString);
        }

        [TestMethod]
        public void TestSegmentGroupAddTags()
        {
            var ediUnaConfig = new EdiUnaConfiguration();
            var subSegments = new EdiBaseSegment[2];

            var rffSegment = new EdiSegment("RFF", 1, ediUnaConfig);
            rffSegment.Add(new[] { "ON", "5" });

            var dtmSegment = new EdiSegment("DTM", 1, ediUnaConfig);
            dtmSegment.Add(new[] { "171", "20170101", "102" });

            subSegments[0] = rffSegment;
            subSegments[1] = dtmSegment;

            var segmentGroup = new EdiSegmentGroup(1, subSegments);
            
            var groupString = segmentGroup.ToString();
            var compareString = "RFF+ON:5'\r\nDTM+171:20170101:102'\r\n";

            Assert.AreEqual(compareString, groupString);
        }

        [TestMethod]
        public void TestSegmentGroupAddGroup()
        {
            var ediUnaConfig = new EdiUnaConfiguration();

            var rffSegment = new EdiSegment("RFF", 1, ediUnaConfig);
            rffSegment.Add(new[] { "ON", "5" });

            var dtmSegment = new EdiSegment("DTM", 1, ediUnaConfig);
            dtmSegment.Add(new[] { "171", "20170101", "102" });

            var moaSegment = new EdiSegment("MOA", 1, ediUnaConfig);
            moaSegment.Add(new[] { "86", "218.39" });

            var group49 = new EdiSegmentGroup(49, new EdiBaseSegment[] { moaSegment });
            var group48Segments = new EdiBaseSegment[] { rffSegment, dtmSegment, group49 };

            var segmentGroup48 = new EdiSegmentGroup(48, group48Segments);

            var groupString = segmentGroup48.ToString();
            var compareString = "RFF+ON:5\'\r\nDTM+171:20170101:102\'\r\nMOA+86:218.39\'\r\n";

            Assert.AreEqual(compareString, groupString);
        }
    }
}
