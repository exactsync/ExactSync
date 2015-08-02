using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace ExactSync.Services
{
    public class DropboxService
    {
        public string AppKey { get; private set; }
        public string AppSecret { get; private set; }
        public string RedirectUri { get; private set; }
        public string AntiForgeryToken { get; private set; }

        public DropboxService()
        {
            this.AppKey = WebConfigurationManager.AppSettings["Dropbox:AppKey"];
            this.AppSecret = WebConfigurationManager.AppSettings["Dropbox:AppSecret"];
            this.RedirectUri = WebConfigurationManager.AppSettings["Dropbox:RedirectUri"];
            this.AntiForgeryToken = Guid.NewGuid().ToString("N");
        }

        public Uri AuthUrl()
        {
            return DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, this.AppKey, this.RedirectUri, this.AntiForgeryToken);
        }

        public async Task<OAuth2Response> AuthAsync(string code)
        {
            return await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, this.AppKey, this.AppSecret, this.RedirectUri);
        }

        public Object[] ProcessDeltaNotification(string jsonData)
        {
            JObject jObject = JObject.Parse(jsonData);
            return jObject["delta"]["users"].ToArray<Object>();
        }

        public async Task<byte[]> DownloadContentAsByteArray(DropboxClient client, DownloadArg arg)
        {
            using (var response = await client.Files.DownloadAsync(arg))
            {
                return await response.GetContentAsByteArrayAsync();
            }
        }
    }
}