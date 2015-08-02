using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExactSync.Services;

namespace ExactSync.Tests.Services
{
    [TestClass]
    public class DropboxServiceTest
    {
        [TestMethod]
        public void TestAuthUrl()
        {
            DropboxService service = new DropboxService();

            Uri expected = new Uri("https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=wo1k230c99d8d7q&redirect_uri=https:%2F%2Flocalhost%2FDropbox%2FAuthorize&state=" + service.AntiForgeryToken);
            Uri actual = service.AuthUrl();

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void TestProcessDeltaNotification()
        {
            string testData = "{\"delta\": {\"users\": [12345678,23456789,11223344]}}";
            string expectedResult_1 = "12345678";
            string expectedResult_2 = "23456789";
            string expectedResult_3 = "11223344";

            DropboxService service = new DropboxService();
            Object[] actual = service.ProcessDeltaNotification(testData);

            Assert.AreEqual(expectedResult_1, actual[0].ToString());
            Assert.AreEqual(expectedResult_2, actual[1].ToString());
            Assert.AreEqual(expectedResult_3, actual[2].ToString());
        }
    }
}
