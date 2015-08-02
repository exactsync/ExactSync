using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ExactSync.Services
{
    public enum OAuthResponseTypeExact
    {
        Code
    }

    public enum Region
    {
        Netherlands,
        Belgium,
        Germany,
        UK,
        US
    }

    public static class ExactOnlineOauth2Helper
    {
        public static string Auth = "/api/oauth2/auth";
        public static string Token = "/api/oauth2/token";

        public static Uri GetAuthorizeUri(OAuthResponseTypeExact oauthResponseType, string clientId, string redirectUri, bool forceLogin = false, Region region = Region.Netherlands)
        {
            var uri = string.IsNullOrEmpty(redirectUri) ? null : new Uri(redirectUri);
            return GetAuthorizeUri(oauthResponseType, clientId, uri, forceLogin, region);
        }

        public static Uri GetAuthorizeUri(OAuthResponseTypeExact oauthResponseType, string clientId, Uri redirectUri, bool forceLogin = false, Region region = Region.Netherlands)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId");
            }
            else if (redirectUri == null)
            {
                throw new ArgumentNullException("redirectUri");
            }
            else if (oauthResponseType != OAuthResponseTypeExact.Code)
            {
                throw new ArgumentNullException("responseType");
            }

            var dict = new Dictionary<string, string>();

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("response_type=");

            switch (oauthResponseType)
            {
                case OAuthResponseTypeExact.Code:
                    queryBuilder.Append("code");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("oauthResponseType");
            }

            queryBuilder.Append("&client_id=").Append(Uri.EscapeDataString(clientId));

            if (redirectUri != null)
            {
                queryBuilder.Append("&redirect_uri=").Append(Uri.EscapeDataString(redirectUri.ToString()));
            }

            string url = String.Concat(ExactOnlineSite(region), Auth);

            var uriBuilder = new UriBuilder(url);
            uriBuilder.Query = queryBuilder.ToString();

            return uriBuilder.Uri;
        }

        public static async Task<OAuth2ResponseExact> ProcessCodeFlowAsync(string code, string clientId, string clientSecret, string redirectUri, Region region = Region.Netherlands, HttpClient client = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            else if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException("clientId");
            }
            else if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException("clientSecret");
            }

            // Null-coalescing operator
            var httpClient = client ?? new HttpClient();

            try
            {
                // x-www-form-urlencoded
                var content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "code", code },
                        { "grant_type", "authorization_code" },
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "redirect_uri", redirectUri }
                    });

                var url = String.Concat(ExactOnlineSite(region), Token);
                var response = await httpClient.PostAsync(url, content);

                var jsonData = await response.Content.ReadAsStringAsync();

                Dictionary<string, string> json = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

                return new OAuth2ResponseExact(
                    json["access_token"].ToString(),
                    json["token_type"].ToString(),
                    Convert.ToInt32(json["expires_in"].ToString()),
                    json["refresh_token"].ToString()
                );
            }
            finally
            {
                // Dispose local new instance only
                if (client == null)
                {
                    httpClient.Dispose();
                }
            }
        }

        public static async Task<OAuth2ResponseExact> ProcessCodeRefreshFlowAsync(string refreshToken, string accessToken, string clientId, string clientSecret, Region region = Region.Netherlands, string tokenType = "bearer", HttpClient client = null)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException("refreshToken");
            }
            else if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException("clientId");
            }
            else if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException("clientSecret");
            }

            // Null-coalescing operator
            var httpClient = client ?? new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

            try
            {
                // x-www-form-urlencoded
                var content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {   
                        { "refresh_token", refreshToken },
                        { "grant_type", "refresh_token" },
                        { "client_id", clientId },
                        { "client_secret", clientSecret }
                    });

                var url = String.Concat(ExactOnlineSite(region), Token);
                var response = await httpClient.PostAsync(url, content);

                var jsonData = await response.Content.ReadAsStringAsync();

                Dictionary<string, string> json = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

                return new OAuth2ResponseExact(
                    json["access_token"].ToString(),
                    json["token_type"].ToString(),
                    Convert.ToInt32(json["expires_in"].ToString()),
                    json["refresh_token"].ToString()
                );
            }
            finally
            {
                // Dispose local new instance only
                if (client == null)
                {
                    httpClient.Dispose();
                }
            }
        }

        public static string ExactOnlineSite(Region region)
        {
            string exactOnlinSite = (region == Region.Netherlands) ? "https://start.exactonline.nl"
                                  : (region == Region.Belgium) ? "https://start.exactonline.be"
                                  : (region == Region.Germany) ? "https://start.exactonline.de"
                                  : (region == Region.UK) ? "https://start.exactonline.co.uk"
                                  : (region == Region.US) ? "https://start.exactonline.com"
                                  : "https://start.exactonline.nl"; //default value
            return exactOnlinSite;
        }
    }

    // Use sealed to prevent inheritance
    public sealed class OAuth2ResponseExact
    {
        internal OAuth2ResponseExact(string access_token, string token_type, int expires_in, string refresh_token)
        {
            if (string.IsNullOrEmpty(access_token))
            {
                throw new ArgumentException("Invalid OAuth 2.0 response, missing access_token and/or uid.");
            }

            this.AccessToken = access_token;
            this.TokenType = token_type;
            this.ExpiresIn = expires_in;
            this.RefreshToken = refresh_token;
        }

        public string AccessToken { get; private set; }
        public string TokenType { get; private set; }
        public int ExpiresIn { get; private set; }
        public string RefreshToken { get; private set; }
    }
}