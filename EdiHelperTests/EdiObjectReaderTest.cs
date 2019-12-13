using System;
using System.Linq;
using EdiHelper;
using EdiHelper.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiHelperTests
{
    [TestClass]
    public class EdiObjectReaderTest
    {
        [TestMethod]
        public void ReadObjectTest()
        {
            var li = new LineItem {ItemNumber = "123456789521245", ItemNumberType = "EAN", PositionCount = 1};
            
            IEdiObjectReader objReader = new EdiObjectReader();
            objReader.Read(li);

            var output = objReader.Get("LIN");

            var innerCollection = output.FirstOrDefault();

            var keyVal = (innerCollection ?? throw new InvalidOperationException()).FirstOrDefault(t => t.Item1.Equals("Count"));

            Assert.AreEqual(keyVal,new Tuple<string, string, int?>("Count","1", null));
        }
    }

    public class LineItem
    {
        [Edi(Tag = "LIN", Placeholder = "Count")]
        public int PositionCount { get; set; }
        [Edi(Tag = "LIN", Placeholder = "xyz")]
        public string ItemNumber { get; set; }
        [Edi(Tag = "LIN", Placeholder = "xyz")]
        public string ItemNumberType { get; set; }
    }
}
