using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace ExactSync.Services
{
    public class ExactOnlineService
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public string CallbackUri { get; private set; }

        public ExactOnlineService()
        {
            this.ClientId = WebConfigurationManager.AppSettings["Exactonline:ClientId"];
            this.ClientSecret = WebConfigurationManager.AppSettings["Exactonline:ClientSecret"];
            this.CallbackUri = WebConfigurationManager.AppSettings["Exactonline:CallbackUri"];
        }

        public Uri AuthUrl()
        {
            return ExactOnlineOauth2Helper.GetAuthorizeUri(OAuthResponseTypeExact.Code, this.ClientId, this.CallbackUri, true, Region.UK);
        }

        public async Task<OAuth2ResponseExact> AuthAsync(string code)
        {
            return await ExactOnlineOauth2Helper.ProcessCodeFlowAsync(code, this.ClientId, this.ClientSecret, this.CallbackUri, Region.UK);
        }

        public async Task<OAuth2ResponseExact> RefreshTokenAsync(string refreshToken, string accessToken)
        {
            return await ExactOnlineOauth2Helper.ProcessCodeRefreshFlowAsync(refreshToken, accessToken, this.ClientId, this.ClientSecret, Region.UK);
        }

        public string ApiEndPoint(Region region = Region.Netherlands)
        {
            return ExactOnlineOauth2Helper.ExactOnlineSite(region);
        }
    }
}