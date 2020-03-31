using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private const string TestxmlWithPlaceholders = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col placeholder=\"unhval1\">1</col>\r\n      </row>\r\n      <row>\r\n        <col placeholder=\"unhval2\">INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segment tag=\"UNB\">\r\n    <rows>\r\n      <row>\r\n        <col>UNOC</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n</segments>";
        private const string TestxmlFullNoGroups = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col>1</col>\r\n      </row>\r\n      <row>\r\n        <col>INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segment tag=\"LIN\">\r\n    <rows>\r\n      <row>\r\n        <col placeholder=\"amount\"></col>\r\n      </row>\r\n      <row>\r\n        <col placeholder=\"price\"></col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n</segments>";
        private const string TestxmlFull = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<segments>\r\n  <segment tag=\"UNH\">\r\n    <rows total=\"2\">\r\n      <row>\r\n        <col>1</col>\r\n      </row>\r\n      <row>\r\n        <col>INVOIC</col>\r\n        <col>D</col>\r\n        <col>96A</col>\r\n        <col>UN</col>\r\n        <col>EAN008</col>\r\n      </row>\r\n    </rows>\r\n  </segment>\r\n  <segmentgroup id=\"1\">\r\n    <segment tag=\"RFF\">\r\n      <rows>\r\n        <row>\r\n          <col placeholder=\"rffph\">RFFPH</col>\r\n        </row>\r\n      </rows>\r\n    </segment>\r\n    <segment tag=\"LIN\">\r\n      <rows>\r\n        <row>\r\n          <col placeholder=\"amount\"></col>\r\n        </row>\r\n        <row>\r\n          <col placeholder=\"price\"></col>\r\n        </row>\r\n      </rows>\r\n    </segment>\r\n  </segmentgroup>\r\n</segments>";

        private const string TestxmlD96A = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><segments><segment tag=\"UNA:\"><rows total=\"1\"><row><col>.?</col></row></rows></segment>  <segment tag=\"UNB\">    <rows total=\"7\">      <row>        <col>UNOC</col>        <col>3</col>      </row>      <row>        <col placeholder=\"GlobalLocationNumber\"></col>        <col>14</col>      </row>      <row>        <col>42604412600005</col>        <col>14</col>      </row>      <row>        <col placeholder=\"InvoiceCreateDate\"></col>        <col placeholder=\"InvoiceCreateTime\"></col>      </row>      <row>        <col placeholder=\"InvoiceNumber\"></col>      </row>      <row>        <col></col>      </row>      <row>        <col>SGH-DUO</col>      </row>    </rows>  </segment>  <segment tag=\"UNH\">    <rows total=\"2\">      <row>        <col>1</col>      </row>      <row>        <col>INVOIC</col>        <col>D</col>        <col>96A</col>        <col>UN</col>        <col>EAN008</col>      </row>    </rows>  </segment>  <segment tag=\"BGM\">    <rows total=\"3\">      <row>        <col>380</col>      </row>      <row>        <col placeholder=\"InvoiceNumber\"></col>      </row>      <row>        <col>9</col>      </row>    </rows>  </segment>  <segment tag=\"DTM\">    <rows total=\"1\">      <row>        <col>137</col>        <col placeholder=\"InvoiceCreateDate\"></col>        <col>102</col>      </row>    </rows>  </segment>  <segmentgroup id=\"2\">    <segment tag=\"NAD\">      <rows total=\"9\">        <row>          <col>SU</col>        </row>        <row>          <col placeholder=\"OwnerGLNNumber\"></col>          <col></col>          <col>92</col>        </row>        <row>          <col></col>        </row>        <row>          <col placeholder=\"OwnerName\"></col>  <col placeholder=\"OwnerName2\"></col>        </row>        <row>          <col placeholder=\"OwnerStreet\"></col>        </row>        <row>          <col placeholder=\"OwnerCity\"></col>        </row>        <row>          <col></col>        </row>        <row>          <col placeholder=\"OwnerPostalCode\"></col>        </row>        <row>          <col placeholder=\"OwnerCountryCode\"></col>        </row>      </rows>    </segment>    <segment tag=\"FII\">      <rows total=\"3\">        <row>          <col>BK</col>        </row>        <row>          <col placeholder=\"OwnerBankAccountIBAN\"></col>          <col placeholder=\"OwnerBankAccountName\"></col>        </row>        <row>          <col placeholder=\"OwnerBankAccountBIC\"></col>          <col></col>          <col></col>          <col></col>          <col></col>          <col></col>          <col placeholder=\"OwnerBankName\"></col>          <col placeholder=\"OwnerBankLocation\"></col>        </row>      </rows>    </segment>    <segmentgroup id=\"3\">      <segment tag=\"RFF\">        <rows total=\"1\">          <row>            <col>VA</col>            <col placeholder=\"OwnerTaxNumber\"></col>          </row>        </rows>      </segment>    </segmentgroup>  </segmentgroup>  <segmentgroup id=\"2\">    <segment tag=\"NAD\">      <rows total=\"9\">        <row>          <col>SU</col>        </row>        <row>          <col placeholder=\"ApplicationAccountingNumber\"></col>          <col></col>          <col>92</col>        </row>        <row>          <col></col>        </row>        <row>          <col placeholder=\"ApplicationName\"></col>        </row>        <row>          <col placeholder=\"ApplicationStreet\"></col>        </row>        <row>          <col placeholder=\"ApplicationCity\"></col>        </row>        <row>          <col></col>        </row>        <row>          <col placeholder=\"ApplicationPostalCode\"></col>        </row>        <row>          <col placeholder=\"ApplicationCountryCode\"></col>        </row>      </rows>    </segment>    <segmentgroup id=\"3\">      <segment tag=\"RFF\">        <rows id=\"1\">          <row>            <col>VA</col>            <col placeholder=\"ApplicationTaxNumber\"></col>          </row>        </rows>      </segment>    </segmentgroup>    <segmentgroup id=\"3\">      <segment tag=\"RFF\">        <rows id=\"1\">          <row>            <col>API</col>            <col placeholder=\"ApplicationAccountingNumber\"></col>          </row>        </rows>      </segment>    </segmentgroup>    <segmentgroup id=\"3\">      <segment tag=\"RFF\">        <rows id=\"1\">          <row>            <col>IT</col>            <col placeholder=\"ApplicationCustomerNumber\"></col>          </row>        </rows>      </segment>    </segmentgroup>  </segmentgroup>  <segmentgroup id=\"6\">    <segment tag=\"TAX\">      <rows total=\"6\">        <row>          <col>7</col>        </row>        <row>          <col>VAT</col>        </row>        <row>          <col></col>        </row>        <row>          <col></col>        </row>        <row>          <col></col>          <col></col>          <col></col>          <col>19.00</col>        </row>        <row>          <col>S</col>        </row>      </rows>    </segment>  </segmentgroup>  <segmentgroup id=\"7\">    <segment tag=\"CUX\">      <rows total=\"1\">        <row>          <col>2</col>          <col>EUR</col>          <col>4</col>        </row>      </rows>    </segment>  </segmentgroup>  <segmentgroup id=\"25\">    <segment tag=\"LIN\">      <rows total=\"3\">        <row>          <col placeholder=\"PositionCounter\"></col>        </row>        <row>          <col></col>        </row>        <row>          <col placeholder=\"PositionCounter\"></col>          <col>GS</col>        </row>      </rows>    </segment><segment tag=\"IMD\"><rows total=\"3\"><row><col>A</col></row><row><col></col></row><row><col></col><col></col><col></col><col placeholder=\"PositionName\"></col><col placeholder=\"PositionName2\"></col></row></rows></segment>    <segment tag=\"QTY\">      <rows total=\"1\">        <row>          <col>46</col>          <col>1</col>          <col>PCE</col>        </row>      </rows>    </segment>    <segment tag=\"MOA\">      <rows total=\"1\">        <row>          <col>203</col>          <col placeholder=\"PositionTotal\"></col>        </row>      </rows>    </segment><segment tag=\"MOA\">      <rows total=\"1\">        <row>          <col>66</col>          <col placeholder=\"PositionTotalNet\"></col>        </row>      </rows>    </segment><segment tag=\"PRI\"><rows total=\"1\"><row><col>AAA</col><col placeholder=\"PositionTotalNet2\"></col><col></col><col></col><col>1</col><col>PCE</col></row></rows></segment><segment tag=\"TAX\"><rows total=\"6\"><row><col>7</col></row><row><col>VAT</col></row><row><col></col></row><row><col></col><col></col><col></col></row><row><col>19</col></row><row><col>S</col></row></rows></segment><segment tag=\"MOA\"><rows total=\"1\"><row><col>124</col><col placeholder=\"PositionTaxTotal\"></col></row></rows></segment>  </segmentgroup>  <segment tag=\"UNS\"><rows total=\"1\"><row><col>S</col></row></rows>  </segment>  <segmentgroup id=\"48\"><segment tag=\"MOA\"><rows total=\"1\"><row><col>86</col><col placeholder=\"SumTotal\"></col></row></rows></segment><segment tag=\"MOA\"><rows total=\"1\"><row><col>125</col><col placeholder=\"SumTotalNet\"></col></row></rows></segment><segment tag=\"MOA\"><rows total=\"1\"><row><col>124</col><col placeholder=\"SumTotalTax\"></col></row></rows></segment>  </segmentgroup>  <segmentgroup id=\"50\"><segment tag=\"TAX\"><rows total=\"6\"><row><col>7</col></row><row><col>VAT</col></row><row><col></col></row><row><col></col><col></col><col></col></row><row><col>19</col></row><row><col>S</col></row></rows></segment><segment tag=\"MOA\"><rows total=\"1\"><row><col>124</col><col placeholder=\"SumTotalTax\"></col></row></rows></segment><segment tag=\"MOA\"><rows total=\"1\"><row><col>125</col><col placeholder=\"SumTotalNet\"></col></row></rows></segment></segmentgroup><segment tag=\"UNT\"><rows total=\"2\"><row><col placeholder=\"SegmentCount\"></col></row><row><col placeholder=\"InvoiceNumber\"></col></row></rows></segment><segment tag=\"UNZ\"><rows total=\"2\"><row><col>1</col></row><row><col placeholder=\"InvoiceNumber\"></col></row></rows></segment></segments>";

        [TestMethod]
        public void ReadD96ADocument()
        {
            var xmlDoc = new XmlDocument();

            byte[] b = Encoding.UTF8.GetBytes(TestxmlD96A);

            using (var s = new MemoryStream(b))
            {
                xmlDoc.Load(s);
            }
            
            if (xmlDoc.DocumentElement == null) return;
            var pos1 = new ApplicationInvoicePosition
            {
                Counter = 1,
                Name = "Verkaufsprovisionen",
                Total = 119,
                TotalNet = 100,
                TotalTax = 19
            };

            var pos2 = new ApplicationInvoicePosition {
                Counter = 2,
                Name = "Monatspauschalen",
                Total = 119,
                TotalNet = 100,
                TotalTax = 19
            };

            var appInv = new ApplicationInvoice
            {
                ApplicationAccountingNumber = "1113",
                ApplicationCity = "Rostock",
                ApplicationCountryCode = "DE",
                ApplicationIdentifier = "4a78baa6-39b8-4632-835b-4ff44ada56fa",
                ApplicationName = "colour and more",
                ApplicationPostalCode = "18055",
                ApplicationStreet = "Kröpeliner Str. 26",
                InvoiceDate = "200325",
                InvoiceDate2 = "20200325",
                InvoiceNumber = "20000",
                InvoiceNumber2 = null,
                InvoiceTime = "0000",
            };

            appInv.Positions.Add(pos1);
            appInv.Positions.Add(pos2);

            //var reader = new EdiObjectReader();
            //var docBuilder = new EdiDocumentBuilder(reader);

            var reader = new EdiObjectReader();
            var docBuilder = new EdiDocumentBuilder(reader);
            var ediDocument = docBuilder.Create(xmlDoc, appInv);
            
            var docString = ediDocument.ToString();

            File.WriteAllText("ediTest.txt", docString);
        }

        public class ApplicationInvoice {
            [Edi(Tag = "NAD", Placeholder = "ApplicationName", Group = 2)]
            public string ApplicationName { get; set; }
            [Edi(Tag = "NAD", Placeholder = "ApplicationStreet", Group = 2)]
            public string ApplicationStreet { get; set; }
            [Edi(Tag = "NAD", Placeholder = "ApplicationPostalCode", Group = 2)]
            public string ApplicationPostalCode { get; set; }
            [Edi(Tag = "NAD", Placeholder = "ApplicationCity", Group = 2)]
            public string ApplicationCity { get; set; }
            [Edi(Tag = "NAD", Placeholder = "ApplicationCountryCode", Group = 2)]
            public string ApplicationCountryCode { get; set; }
            [Edi(Tag = "NAD", Placeholder = "ApplicationAccountingNumber", Group = 2)]
            public string ApplicationAccountingNumber { get; set; }

            public string ApplicationIdentifier { get; set; }


            #region ApplicationInvoice
            [Edi(Tag = "UNB", Placeholder = "InvoiceCreateDate")]
            public string InvoiceDate { get; set; }
            [Edi(Tag = "DTM", Placeholder = "CreateDate")]
            public string InvoiceDate2 { get; set; }
            [Edi(Tag = "UNB", Placeholder = "InvoiceCreateTime")]
            public string InvoiceTime { get; set; }
            [Edi(Tag = "UNB", Placeholder = "InvoiceNumber")]
            public string InvoiceNumber { get; set; }
            [Edi(Tag = "BGM", Placeholder = "InvoiceNumber")]
            public string InvoiceNumber2 { get; set; }

            [EdiList(Group = 25)]
            public List<ApplicationInvoicePosition> Positions { get; }

            public ApplicationInvoice() {
                Positions = new List<ApplicationInvoicePosition>();
            }

            #endregion

            #region Shop Owner
            [Edi(Tag = "NAD", Placeholder = "OwnerName", Group = 2)]
            public string OwnerName { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerName2", Group = 2)]
            public string OwnerName2 { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerStreet", Group = 2)]
            public string OwnerStreet { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerCity", Group = 2)]
            public string OwnerCity { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerPostalCode", Group = 2)]
            public string OwnerPostalcode { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerCountryCode", Group = 2)]
            public string OwnerCountryCode { get; set; }
            [Edi(Tag = "NAD", Placeholder = "OwnerGLNNumber", Group = 2)]
            public string OwnerGLNNumber { get; set; }
            [Edi(Tag = "RFF", Placeholder = "OwnerTaxNumber", Group = 3)]
            public string OwnerTaxNumber { get; set; }

            #endregion

            #region Shop Owner Bank Account
            [Edi(Tag = "FII", Placeholder = "OwnerBankAccountName", Group = 2)]
            public string OwnerBankAccountName { get; set; }
            [Edi(Tag = "FII", Placeholder = "OwnerBankAccountIBAN", Group = 2)]
            public string OwnerBankAccountIBAN { get; set; }
            [Edi(Tag = "FII", Placeholder = "OwnerBankAccountBIC", Group = 2)]
            public string OwnerBankAccountBIC { get; set; }
            [Edi(Tag = "FII", Placeholder = "OwnerBankName", Group = 2)]
            public string OwnerBankName { get; set; }
            [Edi(Tag = "FII", Placeholder = "OwnerBankLocation", Group = 2)]
            public string OwnerBankLocation { get; set; }
            #endregion

        }

        public class ApplicationInvoicePosition {
            [Edi(Tag = "LIN", Placeholder = "PositionCounter", Group = 25)]
            public int Counter { get; set; }
            [Edi(Tag = "IMD", Placeholder = "PositionName", Group = 25)]
            public string Name { get; set; }
            [Edi(Tag = "IMD", Placeholder = "PositionName2", Group = 25)]
            public string Name2 { get; set; }

            [Edi(Tag = "MOA", Placeholder = "PositionTotal", Group = 25)]
            public decimal Total { get; set; }
            [Edi(Tag = "MOA", Placeholder = "PositionTotalNet", Group = 25)]
            public decimal TotalNet { get; set; }
            [Edi(Tag = "PRI", Placeholder = "PositionTotalNet2", Group = 28)]
            public decimal TotalNet2 { get; set; }
            [Edi(Tag = "MOA", Placeholder = "PositionTaxTotal", Group = 33)]
            public decimal TotalTax { get; set; }
        }

        [TestMethod]
        public void ReadXmlDocTest()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlNoGroups);
            
            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReaderDeprecated();

            var docBuilder = new EdiDocumentBuilderDeprecated(reader);
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

            var reader = new EdiObjectReaderDeprecated();

            var docBuilder = new EdiDocumentBuilderDeprecated(reader);
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

            var reader = new EdiObjectReaderDeprecated();

            var docBuilder = new EdiDocumentBuilderDeprecated(reader);
            var o = new MockUpObject() { UNH = "3", SomeValue = "TESTVAL"};
            var ediDocument = docBuilder.Create(xmlDoc, o);

            var docString = ediDocument.ToString();

            var compareString = "UNH+3+TESTVAL:D:96A:UN:EAN008'\r\nUNB+UNOC'\r\n";

            Assert.AreEqual(compareString, docString);
        }

        [TestMethod]
        public void ReadObjectTestIteration()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlFullNoGroups);

            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReaderDeprecated();

            var docBuilder = new EdiDocumentBuilderDeprecated(reader);
            var o = new MockIterable()
            {
                UNH = "3",
                Subs = new List<MockSub>()
                {
                    new MockSub()
                    {
                        Amount = "1",
                        Price = "5"
                    },
                    new MockSub()
                    {
                        Amount = "3",
                        Price = "15"
                    }
                }
            };
            var ediDocument = docBuilder.Create(xmlDoc, o);

            var docString = ediDocument.ToString();

            var compareString = "UNH+1+INVOIC:D:96A:UN:EAN008'\r\nLIN'\r\nLIN'\r\n";

            Assert.AreEqual(compareString, docString);
        }

        [TestMethod]
        public void ReadObjectFullTest()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(TestxmlFull);

            if (xmlDoc.DocumentElement == null) return;

            var reader = new EdiObjectReaderDeprecated();

            var docBuilder = new EdiDocumentBuilderDeprecated(reader);
            var o = new MockIterable()
            {
                UNH = "3",
                Subs = new List<MockSub>()
                {
                    new MockSub()
                    {
                        Amount = "1",
                        Price = "5"
                    },
                    new MockSub()
                    {
                        Amount = "3",
                        Price = "15"
                    }
                }
            };
            var ediDocument = docBuilder.Create(xmlDoc, o);

            var docString = ediDocument.ToString();

            var compareString = "UNH+1+INVOIC:D:96A:UN:EAN008'\r\nRFF+RFFPH'\r\nLIN+1+5'\r\nLIN+3+15'\r\n";

            Assert.AreEqual(compareString, docString);
        }
    }

    public class MockUpObject
    {
        [Edi(Placeholder = "unhval1", Tag = "UNH")]
        public string UNH { get; set; }

        [Edi(Placeholder = "unhval2", Tag = "UNH")]
        public string SomeValue { get; set; }
    }

    public class MockIterable
    {
        [Edi(Placeholder = "unhval1", Tag = "UNH")]
        public string UNH { get; set; }

        [EdiList]
        public ICollection<MockSub> Subs { get; set; }
    }

    public class MockSub
    {
        [Edi(Group = 1, Placeholder = "amount", Tag = "LIN")]
        public string Amount { get; set; }
        [Edi(Group = 1, Placeholder = "price", Tag = "LIN")]
        public string Price { get; set; }
    }
}
