using System;
using System.Collections.Generic;
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

            var keyVal = (innerCollection ?? throw new InvalidOperationException()).FirstOrDefault(kv => kv.Key.Equals("Count"));

            Assert.AreEqual(keyVal,new KeyValuePair<string, string>("Count","1"));
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
