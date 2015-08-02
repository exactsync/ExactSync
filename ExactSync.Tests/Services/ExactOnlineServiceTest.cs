using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExactSync.Services;

namespace ExactSync.Tests.Services
{
    [TestClass]
    public class ExactOnlineServiceTest
    {
        [TestMethod]
        public void TestAuthUrl()
        {
            ExactOnlineService service = new ExactOnlineService();

            Uri expected = new Uri("https://start.exactonline.co.uk/api/oauth2/auth?response_type=code&client_id=6784der2-l98r-3eo0-3333-k3235g98tu75&redirect_uri=https:%2F%2Flocalhost%2FExactOnline%2FAuthorize");
            Uri actual = service.AuthUrl();

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void TestApiEndPoint()
        {
            ExactOnlineService service = new ExactOnlineService();

            string expected = "https://start.exactonline.nl";
            string actual = service.ApiEndPoint();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestApiEndPointBranchUK()
        {
            ExactOnlineService service = new ExactOnlineService();

            string expected = "https://start.exactonline.co.uk";
            string actual = service.ApiEndPoint(Region.UK);

            Assert.AreEqual(expected, actual);
        }
    }
}
