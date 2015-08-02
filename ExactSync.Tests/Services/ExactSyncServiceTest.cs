using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExactSync.Services;

namespace ExactSync.Tests.Services
{
    [TestClass]
    public class ExactSyncServiceTest
    {
        [TestMethod]
        public void TestGetFileName()
        {
            string testData = "/Account/invoice_00001.pdf";
            string expected = "invoice_00001.pdf";

            ExactSyncService service = new ExactSyncService();
            string actual = service.GetFileName(testData);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetFileNameBranchNullArg()
        {
            string testData = null;

            ExactSyncService service = new ExactSyncService();
            string actual = service.GetFileName(testData);

            Assert.IsNull(actual);
        }
    }
}
