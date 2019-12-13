using System.Xml;
using EdiHelper;
using EdiHelper.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdiHelperTests
{
    [TestClass]
    public class EdiBuilderTests
    {
        private const string TestxmlNoGroups = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col>1</col>\r\n      </row>\r\n      <row>\r\n        <col>INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segment tag=\"UNB\">\r\n    <rows>\r\n      <row>\r\n        <col>UNOC</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n</segments>";
        private const string TestxmlWithGroup = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col>1</col>\r\n      </row>\r\n      <row>\r\n        <col>INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segmentgroup id=\"1\">\r\n    <segment tag=\"TST\">\r\n      <rows>\r\n        <row>\r\n          <col>TEST_VALUE</col>\r\n        </row>\r\n      </rows>\r\n    </segment>\r\n  </segmentgroup>\r\n  <segment tag=\"UNB\">\r\n    <rows>\r\n      <row>\r\n        <col>UNOC</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n</segments>";
        private const string TestxmlWithPlaceholders = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col>1</col>\r\n      </row>\r\n      <row>\r\n        <col>INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segmentgroup id=\"1\">\r\n    <segment tag=\"TST\">\r\n      <rows>\r\n        <row>\r\n          <col>TEST_VALUE</col>\r\n        </row>\r\n      </rows>\r\n    </segment>\r\n  </segmentgroup>\r\n  <segment tag=\"UNB\">\r\n    <rows>\r\n      <row>\r\n        <col>UNOC</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n</segments>";

        [TestMethod]
        public void ReadXmlDocTest()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlNoGroups);
            
            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReader();

            var docBuilder = new EdiDocumentBuilder(reader);
            var ediDocument = docBuilder.Create(xmlDoc, new object());

            var docString = ediDocument.ToString();

            var compareString = "UNH+1+INVOIC:D:96A:UN:EAN008'\r\nUNB+UNOC'\r\n";

            Assert.AreEqual(compareString, docString);
        }

        [TestMethod]
        public void ReadXmlDocGroupTest()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlWithGroup);

            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReader();

            var docBuilder = new EdiDocumentBuilder(reader);
            var ediDocument = docBuilder.Create(xmlDoc, new object());

            var docString = ediDocument.ToString();

            var compareString = "UNH+1+INVOIC:D:96A:UN:EAN008'\r\nTST+TEST_VALUE'\r\nUNB+UNOC'\r\n";

            Assert.AreEqual(compareString, docString);
        }

        [TestMethod]
        public void ReadObjectTest()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlWithPlaceholders);

            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReader();

            var docBuilder = new EdiDocumentBuilder(reader);
            var o = new MockUpObject();
            var ediDocument = docBuilder.Create(xmlDoc, o);

            var docString = ediDocument.ToString();

            var compareString = "UNH+1+INVOIC:D:96A:UN:EAN008'\r\nUNB+UNOC'\r\n";

            Assert.AreEqual(compareString, docString);
        }
    }

    public class MockUpObject
    {
        [Edi()]
        public string UNH { get; set; }
    }
}
