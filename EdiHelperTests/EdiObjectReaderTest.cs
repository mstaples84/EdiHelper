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
        public void ReadObjectTestNewReader()
        {
            var pos1 = new EdiBuilderTests.ApplicationInvoicePosition {
                Counter = 1,
                Name = "Verkaufsprovisionen",
                Total = 119,
                TotalNet = 100,
                TotalTax = 19
            };

            var pos2 = new EdiBuilderTests.ApplicationInvoicePosition {
                Counter = 2,
                Name = "Monatspauschalen",
                Total = 119,
                TotalNet = 100,
                TotalTax = 19
            };

            var appInv = new EdiBuilderTests.ApplicationInvoice {
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

            var reader = new EdiObjectReader();
            reader.Read(appInv);

            var result = reader.GetSegmentGroup(25);
        }


        [TestMethod]
        public void ReadObjectTest()
        {
            var li = new LineItem {ItemNumber = "123456789521245", ItemNumberType = "EAN", PositionCount = 1};
            
            IEdiObjectReaderDeprecated objReader = new EdiObjectReaderDeprecated();
            objReader.Read(li);

            var output = objReader.Get("LIN");

            var innerCollection = output.FirstOrDefault();

            var keyVal = (innerCollection ?? throw new InvalidOperationException()).FirstOrDefault(kv => kv.Item1.Equals("Count"));

            Assert.AreEqual(keyVal,new Tuple<string, string, int>("Count","1",0));
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
        public List<EdiBuilderTests.ApplicationInvoicePosition> Positions { get; }

        public ApplicationInvoice() {
            Positions = new List<EdiBuilderTests.ApplicationInvoicePosition>();
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
        [Edi(Tag = "LIN", Placeholder = "PositionCounter")]
        public int Counter { get; set; }
        [Edi(Tag = "IMD", Placeholder = "PositionName")]
        public string Name { get; set; }
        [Edi(Tag = "IMD", Placeholder = "PositionName2")]
        public string Name2 { get; set; }

        [Edi(Tag = "MOA", Placeholder = "PositionTotal")]
        public decimal Total { get; set; }
        [Edi(Tag = "MOA", Placeholder = "PositionTotalNet")]
        public decimal TotalNet { get; set; }
        [Edi(Tag = "PRI", Placeholder = "PositionTotalNet2", Group = 28)]
        public decimal TotalNet2 { get; set; }
        [Edi(Tag = "MOA", Placeholder = "PositionTaxTotal", Group = 33)]
        public decimal TotalTax { get; set; }
    }
}
